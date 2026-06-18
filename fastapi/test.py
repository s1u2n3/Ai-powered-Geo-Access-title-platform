# """
# GeoAccess AI Service — Production Document Ingestion
# =====================================================
# Uses ONLY packages confirmed in your Windows venv pip list.

# Extraction engine (structure-agnostic, no assumptions):
#   1. pdfplumber  — extracts tables with full row/col structure
#   2. pdfminer.six — extracts text blocks with x/y coordinates + font size
#      → font size tells us what is a heading vs body vs caption
#      → x/y position tells us reading order and indentation
#   3. pypdf       — page count, metadata, text-layer detection

# Chunking (no fixed character windows):
#   - Every extracted block gets a type: HEADING / TABLE / TEXT / LIST / CAPTION
#   - Chunks are built by grouping blocks under their nearest heading
#   - Tables always stay as isolated chunks (never merged with text)
#   - Token count tracked per chunk via tiktoken

# No tesseract, no detectron2, no external model needed.
# Works on: digital PDFs, government forms, bank letters, structured reports.
# """

# import os
# import re
# import sys
# import types
# import uuid
# import logging
# import tempfile
# from enum import Enum
# from typing import Optional
# from dataclasses import dataclass, field

# # ── Stub unstructured's optional imports so it doesn't crash on Windows ──────
# def _stub(name: str, attrs: dict | None = None):
#     if name not in sys.modules:
#         m = types.ModuleType(name)
#         for k, v in (attrs or {}).items():
#             setattr(m, k, v)
#         sys.modules[name] = m

# _stub("pi_heif",     {"register_heif_opener": lambda *a, **kw: None})
# _stub("detectron2")
# _stub("layoutparser")
# # ─────────────────────────────────────────────────────────────────────────────

# import pdfplumber
# import tiktoken
# from pypdf import PdfReader
# from pdfminer.high_level import extract_pages
# from pdfminer.layout import (
#     LTPage, LTTextBox, LTTextLine, LTChar,
#     LTFigure, LTRect, LTLine, LTAnno
# )

# from fastapi import FastAPI, File, HTTPException, UploadFile
# from pydantic import BaseModel
# import requests

# logging.basicConfig(
#     level=logging.INFO,
#     format="%(asctime)s [%(levelname)s] %(name)s — %(message)s",
# )
# log = logging.getLogger("geoaccess")

# # tiktoken encoder — cl100k_base is used by OpenAI text-embedding-3-large
# _ENC = tiktoken.get_encoding("cl100k_base")

# def token_count(text: str) -> int:
#     return len(_ENC.encode(text))


# # ─────────────────────────────────────────────────────────────────────────────
# # CONFIG — tune without touching logic
# # ─────────────────────────────────────────────────────────────────────────────

# class Config:
#     # Text-layer coverage thresholds
#     DIGITAL_THRESHOLD = 0.85
#     SCANNED_THRESHOLD = 0.10
#     MIN_TEXT_CHARS_PER_PAGE = 40

#     # Font-size thresholds for heading detection
#     # pdfminer gives actual font sizes in points
#     HEADING_FONT_RATIO   = 1.15   # font >= 1.15x median → heading
#     CAPTION_FONT_RATIO   = 0.85   # font <= 0.85x median → caption/footnote

#     # Chunking limits (in tokens — matches embedding model context)
#     MAX_TOKENS_PER_CHUNK  = 400   # hard ceiling
#     SOFT_TOKEN_TARGET     = 300   # try to stay under this
#     MIN_TOKENS_TO_KEEP    = 20    # discard chunks smaller than this

#     # pdfplumber table detection — tries line-based first, text-based fallback
#     TABLE_SETTINGS_LINES = {
#         "vertical_strategy":    "lines",
#         "horizontal_strategy":  "lines",
#         "snap_tolerance":       4,
#         "join_tolerance":       4,
#         "edge_min_length":      8,
#         "min_words_vertical":   1,
#         "min_words_horizontal": 1,
#     }
#     TABLE_SETTINGS_TEXT = {
#         "vertical_strategy":    "text",
#         "horizontal_strategy":  "text",
#         "snap_tolerance":       4,
#         "join_tolerance":       4,
#         "text_x_tolerance":     4,
#         "text_y_tolerance":     4,
#     }


# # ─────────────────────────────────────────────────────────────────────────────
# # INTERNAL DATA STRUCTURES
# # ─────────────────────────────────────────────────────────────────────────────

# class BlockType(str, Enum):
#     HEADING  = "Heading"
#     TEXT     = "Text"
#     TABLE    = "Table"
#     LIST     = "List"
#     CAPTION  = "Caption"
#     KV_BLOCK = "KeyValue"


# @dataclass
# class Block:
#     """One extracted unit from the PDF — could be a heading, paragraph, table, etc."""
#     block_type: BlockType
#     text:       str
#     page:       int
#     font_size:  float = 0.0
#     x0:         float = 0.0   # left position — useful for indent detection
#     y0:         float = 0.0   # top position  — reading order


# # ─────────────────────────────────────────────────────────────────────────────
# # PYDANTIC RESPONSE MODELS
# # ─────────────────────────────────────────────────────────────────────────────

# class DocumentType(str, Enum):
#     DIGITAL = "digital"
#     SCANNED = "scanned"
#     MIXED   = "mixed"


# class ChunkMeta(BaseModel):
#     chunk_id:      str
#     chunk_order:   int
#     section:       Optional[str]   # heading this chunk falls under
#     block_types:   list[str]       # e.g. ["Heading","Text","KeyValue"]
#     pages:         list[int]
#     has_table:     bool
#     token_count:   int
#     char_count:    int


# class Chunk(BaseModel):
#     chunk_id:    str
#     chunk_order: int
#     text:        str
#     metadata:    ChunkMeta


# class Result(BaseModel):
#     file_name:  str
#     doc_id:     str
#     doc_type:   DocumentType
#     strategy:   str
#     block_count: int
#     chunk_count: int
#     chunks:     list[Chunk]
    
# class EmbeddedChunk(Chunk):
#     embedding: list[float]


# class EmbeddingResult(BaseModel):
#     file_name: str
#     doc_id: str
#     doc_type: DocumentType
#     strategy: str
#     block_count: int
#     chunk_count: int
#     chunks: list[EmbeddedChunk]





# # ─────────────────────────────────────────────────────────────────────────────
# # STEP 1 — Document type detection (pypdf, no fitz)
# # ─────────────────────────────────────────────────────────────────────────────

# def detect_type(path: str) -> tuple[DocumentType, float]:
#     try:
#         r = PdfReader(path)
#         total = len(r.pages)
#         if total == 0:
#             return DocumentType.SCANNED, 0.0
#         with_text = sum(
#             1 for p in r.pages
#             if len((p.extract_text() or "").strip()) >= Config.MIN_TEXT_CHARS_PER_PAGE
#         )
#         cov = with_text / total
#         log.info(f"Coverage: {cov:.0%} ({with_text}/{total} pages)")
#         if cov >= Config.DIGITAL_THRESHOLD:
#             return DocumentType.DIGITAL, cov
#         if cov <= Config.SCANNED_THRESHOLD:
#             return DocumentType.SCANNED, cov
#         return DocumentType.MIXED, cov
#     except Exception as e:
#         log.warning(f"Type detection failed: {e}")
#         return DocumentType.MIXED, 0.0


# # ─────────────────────────────────────────────────────────────────────────────
# # STEP 2A — Table extraction via pdfplumber
# #
# # pdfplumber finds tables by detecting PDF line geometry (borders).
# # Falls back to text-alignment-based detection when no ruled borders exist.
# # Each table becomes ONE Block — rows joined as pipe-delimited text.
# # Returns: {page_number: [table_bbox, ...]} so text extractor can skip them.
# # ─────────────────────────────────────────────────────────────────────────────

# def extract_tables(path: str) -> tuple[list[Block], dict[int, list[tuple]]]:
#     """
#     Returns:
#       table_blocks : list of Block(TABLE) with pipe-delimited text
#       skip_zones   : {1-based page number → [(x0,y0,x1,y1), ...]}
#                      Text extractor must skip these zones to avoid double-extraction.
#     """
#     table_blocks: list[Block] = []
#     skip_zones:   dict[int, list[tuple]] = {}

#     with pdfplumber.open(path) as pdf:
#         for page_num, page in enumerate(pdf.pages, start=1):

#             # Try line-based detection first (better for ruled tables like govt forms)
#             try:
#                 tables     = page.extract_tables(Config.TABLE_SETTINGS_LINES)
#                 table_objs = page.find_tables(Config.TABLE_SETTINGS_LINES)
#             except Exception:
#                 tables     = []
#                 table_objs = []

#             # Fallback to text-alignment if no line-based tables found
#             if not tables:
#                 try:
#                     tables     = page.extract_tables(Config.TABLE_SETTINGS_TEXT)
#                     table_objs = page.find_tables(Config.TABLE_SETTINGS_TEXT)
#                 except Exception:
#                     tables     = []
#                     table_objs = []

#             # Register bounding boxes so text extractor skips them
#             skip_zones[page_num] = []
#             for tobj in table_objs:
#                 try:
#                     skip_zones[page_num].append(tobj.bbox)
#                 except Exception:
#                     pass

#             for table in tables:
#                 rows = []
#                 for row in table:
#                     cells = [str(c).strip() if c is not None else "" for c in row]
#                     if any(cells):
#                         rows.append(" | ".join(cells))
#                 if rows:
#                     table_blocks.append(Block(
#                         block_type=BlockType.TABLE,
#                         text="\n".join(rows),
#                         page=page_num,
#                     ))

#     log.info(f"Tables found: {len(table_blocks)}")
#     return table_blocks, skip_zones


# # ─────────────────────────────────────────────────────────────────────────────
# # STEP 2B — Text extraction via pdfminer (layout-aware)
# #
# # pdfminer gives us:
# #   - LTTextBox: a group of lines that belong together spatially
# #   - LTChar: individual characters with font name + size
# #
# # We use font size relative to the page median to classify:
# #   - Large font  → HEADING
# #   - Normal font → TEXT or KV_BLOCK or LIST
# #   - Small font  → CAPTION
# #
# # We skip any text whose bounding box overlaps a table zone (from Step 2A).
# # ─────────────────────────────────────────────────────────────────────────────

# def _dominant_font_size(textbox: LTTextBox) -> float:
#     """Return the most common font size in this text box."""
#     sizes: list[float] = []
#     for line in textbox:
#         if isinstance(line, LTTextLine):
#             for ch in line:
#                 if isinstance(ch, LTChar) and ch.size > 0:
#                     sizes.append(round(ch.size, 1))
#     if not sizes:
#         return 0.0
#     # Most frequent size
#     return max(set(sizes), key=sizes.count)


# def _overlaps_table(box: tuple, zones: list[tuple], tolerance: float = 2.0) -> bool:
#     """True if box overlaps any table zone (with small tolerance for PDF coord noise)."""
#     x0, y0, x1, y1 = box
#     for (tx0, ty0, tx1, ty1) in zones:
#         if x0 < tx1 - tolerance and x1 > tx0 + tolerance \
#            and y0 < ty1 - tolerance and y1 > ty0 + tolerance:
#             return True
#     return False


# def _classify_text(text: str, font_size: float, median_size: float, x0: float) -> BlockType:
#     """
#     Classify a text block purely from visual signals — no keyword assumptions.
#     Works on any document type, any language, any structure.
#     """
#     text = text.strip()
#     lines = [l.strip() for l in text.splitlines() if l.strip()]

#     if not lines:
#         return BlockType.TEXT

#     # Font-size signals
#     if median_size > 0:
#         ratio = font_size / median_size
#         if ratio >= Config.HEADING_FONT_RATIO:
#             return BlockType.HEADING
#         if ratio <= Config.CAPTION_FONT_RATIO:
#             return BlockType.CAPTION

#     # Key-value heuristic: multiple short lines, each looks like "Label  Value"
#     # Works for any language — just checks line count and avg word count
#     if len(lines) >= 2:
#         avg_words = sum(len(l.split()) for l in lines) / len(lines)
#         if 2 <= avg_words <= 12 and all(len(l) < 120 for l in lines):
#             # Check if lines have internal whitespace gaps (KV spacing)
#             gap_lines = sum(1 for l in lines if re.search(r'\s{2,}', l))
#             if gap_lines >= len(lines) * 0.5:
#                 return BlockType.KV_BLOCK

#     # List heuristic: lines starting with bullet-like chars or numbers
#     list_starters = sum(
#         1 for l in lines
#         if re.match(r'^[\-\•\*\u2022\u25cf\d]+[\.\)]\s', l)
#     )
#     if list_starters >= len(lines) * 0.5:
#         return BlockType.LIST

#     # Single short line with no sentence-ending punctuation mid-line → heading candidate
#     if len(lines) == 1 and len(text) < 100 and not text.endswith(","):
#         words = text.split()
#         if 1 <= len(words) <= 10:
#             return BlockType.HEADING

#     return BlockType.TEXT


# def extract_text_blocks(
#     path: str,
#     skip_zones: dict[int, list[tuple]]
# ) -> list[Block]:
#     """
#     Use pdfminer to extract all text boxes outside table zones.
#     Classify each box by font size + content patterns.
#     """
#     all_blocks: list[Block] = []

#     for page_layout in extract_pages(path):
#         if not isinstance(page_layout, LTPage):
#             continue

#         page_num = page_layout.pageid   # 1-based
#         zones    = skip_zones.get(page_num, [])

#         # Collect all text boxes on this page with font sizes
#         page_boxes: list[tuple[float, float, float, LTTextBox]] = []
#         for element in page_layout:
#             if not isinstance(element, LTTextBox):
#                 continue
#             text = element.get_text().strip()
#             if not text:
#                 continue
#             fs = _dominant_font_size(element)
#             page_boxes.append((element.x0, element.y0, fs, element))

#         if not page_boxes:
#             continue

#         # Compute median font size for this page (used for heading detection)
#         font_sizes = [fs for _, _, fs, _ in page_boxes if fs > 0]
#         median_fs  = sorted(font_sizes)[len(font_sizes) // 2] if font_sizes else 10.0

#         for x0, y0, fs, element in page_boxes:
#             text = element.get_text().strip()
#             bbox = (element.x0, element.y0, element.x1, element.y1)

#             # Skip if this box is inside a table zone
#             if _overlaps_table(bbox, zones):
#                 continue

#             block_type = _classify_text(text, fs, median_fs, x0)

#             all_blocks.append(Block(
#                 block_type=block_type,
#                 text=text,
#                 page=page_num,
#                 font_size=fs,
#                 x0=x0,
#                 y0=y0,
#             ))

#     log.info(f"Text blocks found: {len(all_blocks)}")
#     return all_blocks


# # ─────────────────────────────────────────────────────────────────────────────
# # STEP 2C — Merge and sort all blocks into reading order
# #
# # PDF pages have blocks in arbitrary order in the file.
# # Sort by (page, y_descending, x_ascending) to match natural reading order.
# # y is inverted because PDF y=0 is bottom of page.
# # ─────────────────────────────────────────────────────────────────────────────

# def merge_and_sort(
#     text_blocks: list[Block],
#     table_blocks: list[Block],
# ) -> list[Block]:
#     """
#     Combine text and table blocks, sort into reading order.
#     Tables are inserted at the y-position where pdfplumber found them.
#     For tables without y coordinates, they're appended after text blocks on same page.
#     """
#     all_blocks = text_blocks + table_blocks
#     # Sort: page first, then top-to-bottom (higher y0 = higher on page in PDF coords)
#     # pdfminer y0 is from bottom, so DESCENDING y0 = top of page first
#     all_blocks.sort(key=lambda b: (b.page, -b.y0, b.x0))
#     return all_blocks


# # ─────────────────────────────────────────────────────────────────────────────
# # STEP 3 — Semantic chunking
# #
# # Strategy:
# #   1. Walk blocks in reading order
# #   2. HEADING → flush current buffer, start new chunk
# #   3. TABLE   → flush current buffer, TABLE becomes its own chunk, resume
# #   4. TEXT/KV/LIST/CAPTION → accumulate in buffer
# #   5. If buffer exceeds MAX_TOKENS → flush and continue
# #
# # This means every chunk is semantically bounded by document structure,
# # not by an arbitrary character window.
# # ─────────────────────────────────────────────────────────────────────────────

# def build_chunks(blocks: list[Block]) -> list[Chunk]:
#     results: list[Chunk] = []
#     order   = 0

#     buffer:          list[Block] = []
#     current_section: Optional[str] = None

#     def flush(buf: list[Block]) -> None:
#         nonlocal order
#         if not buf:
#             return
#         text = "\n\n".join(b.text for b in buf).strip()
#         tc   = token_count(text)
#         if tc < Config.MIN_TOKENS_TO_KEEP:
#             return

#         # Collect section title: last HEADING in buffer, or current_section
#         headings = [b.text for b in buf if b.block_type == BlockType.HEADING]
#         section  = headings[-1] if headings else current_section

#         cid = str(uuid.uuid4())
#         results.append(Chunk(
#             chunk_id=cid,
#             chunk_order=order,
#             text=text,
#             metadata=ChunkMeta(
#                 chunk_id=cid,
#                 chunk_order=order,
#                 section=section,
#                 block_types=list(dict.fromkeys(b.block_type.value for b in buf)),
#                 pages=sorted({b.page for b in buf}),
#                 has_table=any(b.block_type == BlockType.TABLE for b in buf),
#                 token_count=tc,
#                 char_count=len(text),
#             ),
#         ))
#         order += 1

#     for block in blocks:

#         # TABLE always gets its own isolated chunk
#         if block.block_type == BlockType.TABLE:
#             flush(buffer)
#             buffer = []
#             flush([block])
#             continue

#         # HEADING → new chunk boundary
#         if block.block_type == BlockType.HEADING:
#             flush(buffer)
#             buffer = [block]
#             current_section = block.text
#             continue

#         # Check token ceiling before accumulating
#         current_tokens = token_count("\n\n".join(b.text for b in buffer)) if buffer else 0
#         new_tokens     = token_count(block.text)

#         if current_tokens + new_tokens > Config.MAX_TOKENS_PER_CHUNK and buffer:
#             flush(buffer)
#             buffer = [block]
#         else:
#             buffer.append(block)

#     flush(buffer)

#     log.info(f"Chunks built: {len(results)}")
#     return results


# # ─────────────────────────────────────────────────────────────────────────────
# # MAIN EXTRACTION PIPELINE
# # ─────────────────────────────────────────────────────────────────────────────

# def process_pdf(path: str) -> tuple[DocumentType, str, list[Block], list[Chunk]]:
#     # 1. Detect doc type
#     doc_type, coverage = detect_type(path)

#     if doc_type == DocumentType.SCANNED:
#         raise ValueError(
#             "Fully scanned PDF detected (no text layer). "
#             "Pre-process with ocrmypdf or similar OCR tool first."
#         )

#     strategy = f"pdfplumber (tables) + pdfminer (layout text) | type={doc_type.value} coverage={coverage:.0%}"

#     # 2. Extract tables first, get zones to skip in text pass
#     table_blocks, skip_zones = extract_tables(path)

#     # 3. Extract text blocks (skipping table zones)
#     text_blocks = extract_text_blocks(path, skip_zones)

#     # 4. Merge into reading order
#     all_blocks = merge_and_sort(text_blocks, table_blocks)

#     # 5. Build semantic chunks
#     chunks = build_chunks(all_blocks)

#     return doc_type, strategy, all_blocks, chunks


# # ─────────────────────────────────────────────────────────────────────────────
# # FASTAPI
# # ─────────────────────────────────────────────────────────────────────────────

# app = FastAPI(
#     title="GeoAccess AI Service",
#     version="4.0",
#     description=(
#         "Structure-agnostic PDF extraction. "
#         "Tables, KV pairs, headings, paragraphs — all extracted and chunked "
#         "without any assumptions about document structure."
#     ),
# )


# @app.get("/")
# def home():
#     return {"message": "GeoAccess AI Service", "version": "4.0"}


# @app.get("/health")
# def health():
#     return {"status": "healthy"}


# @app.post("/process-document", response_model=Result)
# async def process_document(file: UploadFile = File(...)):
#     if not file.filename.lower().endswith(".pdf"):
#         raise HTTPException(400, "Only PDF files supported.")

#     tmp_path: Optional[str] = None
#     try:
#         with tempfile.NamedTemporaryFile(delete=False, suffix=".pdf") as tmp:
#             tmp.write(await file.read())
#             tmp_path = tmp.name

#         doc_id = str(uuid.uuid4())
#         log.info(f"Processing {file.filename} → {doc_id}")

#         doc_type, strategy, blocks, chunks = process_pdf(tmp_path)

#         return Result(
#             file_name=file.filename,
#             doc_id=doc_id,
#             doc_type=doc_type,
#             strategy=strategy,
#             block_count=len(blocks),
#             chunk_count=len(chunks),
#             chunks=chunks,
#         )

#     except HTTPException:
#         raise
#     except ValueError as ve:
#         raise HTTPException(422, str(ve))
#     except Exception as ex:
#         log.exception(f"Failed: {file.filename}")
#         raise HTTPException(500, str(ex))
#     finally:
#         if tmp_path and os.path.exists(tmp_path):
#             os.remove(tmp_path)
         


# def generate_embedding(text: str):

#     response = requests.post(
#         "http://localhost:11434/api/embeddings",
#         json={
#             "model": "nomic-embed-text",
#             "prompt": text
#         },
#         timeout=120
#     )

#     response.raise_for_status()

#     return response.json()["embedding"]


# def build_embedding_text(
#     document_type: str,
#     section: str | None,
#     chunk_text: str
# ):
#     return f"""
# Document Type: {document_type}

# Section: {section or "General"}

# {chunk_text}
# """
# @app.post(
#     "/process-document-with-embeddings",
#     response_model=EmbeddingResult
# )

# async def process_document_with_embeddings(
#     file: UploadFile = File(...)
# ):
#     if not file.filename.lower().endswith(".pdf"):
#         raise HTTPException(
#             status_code=400,
#             detail="Only PDF files supported."
#         )

#     tmp_path = None

#     try:
#         with tempfile.NamedTemporaryFile(
#             delete=False,
#             suffix=".pdf"
#         ) as tmp:

#             tmp.write(await file.read())
#             tmp_path = tmp.name

#         doc_id = str(uuid.uuid4())

#         doc_type, strategy, blocks, chunks = process_pdf(
#             tmp_path
#         )

#         embedded_chunks = []

#         for chunk in chunks:

#             embedding_text = build_embedding_text(
#                 doc_type.value,
#                 chunk.metadata.section,
#                 chunk.text
#             )

#             embedding = generate_embedding(
#                 embedding_text
#             )

#             embedded_chunks.append(
#                 EmbeddedChunk(
#                     chunk_id=chunk.chunk_id,
#                     chunk_order=chunk.chunk_order,
#                     text=chunk.text,
#                     embedding=embedding,
#                     metadata=chunk.metadata
#                 )
#             )

#         return EmbeddingResult(
#             file_name=file.filename,
#             doc_id=doc_id,
#             doc_type=doc_type,
#             strategy=strategy,
#             block_count=len(blocks),
#             chunk_count=len(embedded_chunks),
#             chunks=embedded_chunks
#         )

#     except HTTPException:
#         raise

#     except Exception as ex:
#         log.exception(ex)

#         raise HTTPException(
#             status_code=500,
#             detail=str(ex)
#         )

#     finally:
#         if tmp_path and os.path.exists(tmp_path):
#             os.remove(tmp_path)



