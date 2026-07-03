"""
GeoAccess AI Service — Production Document Ingestion
=====================================================
v5: Adds OCR fallback for scanned/image-based PDFs and standalone images,
while keeping the ORIGINAL digital-PDF extraction pipeline (pdfplumber +
pdfminer.six + pypdf) completely intact and unchanged.

Extraction engine (structure-agnostic, no assumptions):
  DIGITAL PAGES (unchanged from original):
    1. pdfplumber  — extracts tables with full row/col structure
    2. pdfminer.six — extracts text blocks with x/y coordinates + font size
       → font size tells us what is a heading vs body vs caption
       → x/y position tells us reading order and indentation
    3. pypdf       — page count, metadata, text-layer detection

  SCANNED / IMAGE PAGES (new):
    4. PyMuPDF (fitz) — renders PDF pages to images (no poppler needed)
    5. OpenCV         — preprocessing: deskew, denoise, CLAHE contrast,
                         adaptive threshold (handles xerox/blurred/noisy)
    6. pytesseract    — OCR text + word-level bounding boxes
       → bounding box height is used as a font-size proxy for
         heading/caption/KV/list classification (same heuristics as digital)
    7. OpenCV line detection — detects ruled table grids on scanned pages
       and groups OCR words into rows/cols -> pipe-delimited TABLE blocks

ROUTING (per-page, not whole-document):
  - Each page is checked individually for a real text layer.
  - Pages WITH text layer    -> original pdfplumber + pdfminer path
  - Pages WITHOUT text layer -> render to image -> preprocess -> OCR path
  - Standalone images (.jpg/.png/.tiff/.bmp) -> preprocess -> OCR path
  - Both paths produce the SAME Block objects, merged + sorted + chunked
    by the ORIGINAL merge_and_sort / build_chunks logic (unchanged).

Chunking (no fixed character windows) — UNCHANGED:
  - Every extracted block gets a type: HEADING / TABLE / TEXT / LIST / CAPTION / KeyValue
  - Chunks are built by grouping blocks under their nearest heading
  - Tables always stay as isolated chunks (never merged with text)
  - Token count tracked per chunk via tiktoken
"""

import os
import re
import sys
import types
import uuid
import logging
import tempfile
from enum import Enum
from typing import Optional
from dataclasses import dataclass, field
import requests

# ── Stub unstructured's optional imports so it doesn't crash on Windows ──────
def _stub(name: str, attrs: dict | None = None):
    if name not in sys.modules:
        m = types.ModuleType(name)
        for k, v in (attrs or {}).items():
            setattr(m, k, v)
        sys.modules[name] = m

_stub("pi_heif",     {"register_heif_opener": lambda *a, **kw: None})
_stub("detectron2")
_stub("layoutparser")
# ─────────────────────────────────────────────────────────────────────────────

import pdfplumber
import tiktoken
from pypdf import PdfReader
from pdfminer.high_level import extract_pages
from pdfminer.layout import (
    LTPage, LTTextBox, LTTextLine, LTChar,
    LTFigure, LTRect, LTLine, LTAnno
)

from fastapi import FastAPI, File, HTTPException, UploadFile
from pydantic import BaseModel
import requests

# ── NEW: OCR-path imports ─────────────────────────────────────────────────────
import numpy as np
import cv2
import fitz  # PyMuPDF — renders PDF pages to images, no poppler dependency
import pytesseract
# ───────────────────────────────────────────────────────────────────────────────

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(name)s — %(message)s",
)
log = logging.getLogger("geoaccess")

# tiktoken encoder — cl100k_base is used by OpenAI text-embedding-3-large
_ENC = tiktoken.get_encoding("cl100k_base")

def token_count(text: str) -> int:
    return len(_ENC.encode(text))


# ─────────────────────────────────────────────────────────────────────────────
# CONFIG — tune without touching logic
# ─────────────────────────────────────────────────────────────────────────────

class Config:
    # Text-layer coverage thresholds (used for whole-doc strategy label)
    DIGITAL_THRESHOLD = 0.85
    SCANNED_THRESHOLD = 0.10
    MIN_TEXT_CHARS_PER_PAGE = 40

    # Font-size thresholds for heading detection
    # pdfminer gives actual font sizes in points
    HEADING_FONT_RATIO   = 1.15   # font >= 1.15x median → heading
    CAPTION_FONT_RATIO   = 0.85   # font <= 0.85x median → caption/footnote

    # Chunking limits (in tokens — matches embedding model context)
    MAX_TOKENS_PER_CHUNK  = 400   # hard ceiling
    SOFT_TOKEN_TARGET     = 300   # try to stay under this
    MIN_TOKENS_TO_KEEP    = 10  # discard chunks smaller than this

    # pdfplumber table detection — tries line-based first, text-based fallback
    TABLE_SETTINGS_LINES = {
        "vertical_strategy":    "lines",
        "horizontal_strategy":  "lines",
        "snap_tolerance":       4,
        "join_tolerance":       4,
        "edge_min_length":      8,
        "min_words_vertical":   1,
        "min_words_horizontal": 1,
    }
    TABLE_SETTINGS_TEXT = {
        "vertical_strategy":    "text",
        "horizontal_strategy":  "text",
        "snap_tolerance":       4,
        "join_tolerance":       4,
        "text_x_tolerance":     4,
        "text_y_tolerance":     4,
    }

    # ── NEW: OCR-path config ──────────────────────────────────────────────
    # Render DPI when converting PDF page -> image for OCR
    OCR_RENDER_DPI = 250

    # OCR-path heading/caption detection — based on relative bbox height
    # (proxy for font size, since OCR gives pixel boxes not point sizes)
    OCR_HEADING_HEIGHT_RATIO = 1.4   # line height >= 1.4x median -> heading
    OCR_CAPTION_HEIGHT_RATIO = 0.7

    # Tesseract languages (space/plus separated, e.g. "eng+hin+tam")
    OCR_LANG = os.environ.get("TESSERACT_LANG", "eng")

    # Path to tesseract binary on Windows (only used if set / exists)
    TESSERACT_CMD_WIN =  r"C:\Users\sunda\AppData\Local\Programs\Tesseract-OCR\tesseract.exe"

    # OCR table-grid detection (OpenCV line detection on scanned pages)
    TABLE_LINE_MIN_LENGTH_RATIO = 0.25  # detected line must span >= 25% of page dimension
    TABLE_MIN_ROWS = 2
    TABLE_MIN_COLS = 2

    # Discard OCR words with very low confidence (Tesseract gives 0-100)
    OCR_MIN_WORD_CONFIDENCE = 30


# ─────────────────────────────────────────────────────────────────────────────
# INTERNAL DATA STRUCTURES
# ─────────────────────────────────────────────────────────────────────────────

class BlockType(str, Enum):
    HEADING  = "Heading"
    TEXT     = "Text"
    TABLE    = "Table"
    LIST     = "List"
    CAPTION  = "Caption"
    KV_BLOCK = "KeyValue"


@dataclass
class Block:
    """One extracted unit from the PDF — could be a heading, paragraph, table, etc."""
    block_type: BlockType
    text:       str
    page:       int
    font_size:  float = 0.0
    x0:         float = 0.0   # left position — useful for indent detection
    y0:         float = 0.0   # top position  — reading order
    source:     str   = "digital"   # NEW: "digital" | "ocr"


# ─────────────────────────────────────────────────────────────────────────────
# PYDANTIC RESPONSE MODELS
# ─────────────────────────────────────────────────────────────────────────────

class DocumentType(str, Enum):
    DIGITAL = "digital"
    SCANNED = "scanned"
    MIXED   = "mixed"


class ChunkMeta(BaseModel):
    chunk_id:      str
    chunk_order:   int
    section:       Optional[str]   # heading this chunk falls under
    block_types:   list[str]       # e.g. ["Heading","Text","KeyValue"]
    pages:         list[int]
    has_table:     bool
    token_count:   int
    char_count:    int
    source:        str             # NEW: "digital" | "ocr" | "mixed"


class Chunk(BaseModel):
    chunk_id:    str
    chunk_order: int
    text:        str
    metadata:    ChunkMeta


class Result(BaseModel):
    file_name:  str
    doc_id:     str
    doc_type:   DocumentType
    strategy:   str
    block_count: int
    chunk_count: int
    chunks:     list[Chunk]

class EmbeddedChunk(Chunk):
    embedding: list[float]


class EmbeddingResult(BaseModel):
    file_name: str
    doc_id: str
    doc_type: DocumentType
    strategy: str
    block_count: int
    chunk_count: int
    chunks: list[EmbeddedChunk]


# ─────────────────────────────────────────────────────────────────────────────
# STEP 1 — Document type detection (pypdf, no fitz)  — UNCHANGED
# ─────────────────────────────────────────────────────────────────────────────

def detect_type(path: str) -> tuple[DocumentType, float]:
    try:
        r = PdfReader(path)
        total = len(r.pages)
        if total == 0:
            return DocumentType.SCANNED, 0.0
        with_text = sum(
            1 for p in r.pages
            if len((p.extract_text() or "").strip()) >= Config.MIN_TEXT_CHARS_PER_PAGE
        )
        cov = with_text / total
        log.info(f"Coverage: {cov:.0%} ({with_text}/{total} pages)")
        if cov >= Config.DIGITAL_THRESHOLD:
            return DocumentType.DIGITAL, cov
        if cov <= Config.SCANNED_THRESHOLD:
            return DocumentType.SCANNED, cov
        return DocumentType.MIXED, cov
    except Exception as e:
        log.warning(f"Type detection failed: {e}")
        return DocumentType.MIXED, 0.0


# ─────────────────────────────────────────────────────────────────────────────
# NEW — Per-page text-layer detection (for routing digital vs OCR per page)
# ─────────────────────────────────────────────────────────────────────────────

def detect_page_text_layers(path: str) -> list[bool]:
    """
    Returns a list, one entry per page (0-indexed), True if that page has a
    real text layer (digital), False if it needs OCR.
    """
    r = PdfReader(path)
    flags = []
    for p in r.pages:
        chars = len((p.extract_text() or "").strip())
        flags.append(chars >= Config.MIN_TEXT_CHARS_PER_PAGE)
    return flags


# ─────────────────────────────────────────────────────────────────────────────
# STEP 2A — Table extraction via pdfplumber  — UNCHANGED
#
# pdfplumber finds tables by detecting PDF line geometry (borders).
# Falls back to text-alignment-based detection when no ruled borders exist.
# Each table becomes ONE Block — rows joined as pipe-delimited text.
# Returns: {page_number: [table_bbox, ...]} so text extractor can skip them.
# ─────────────────────────────────────────────────────────────────────────────

def extract_tables(path: str, pages_filter: Optional[set[int]] = None) -> tuple[list[Block], dict[int, list[tuple]]]:
    """
    Returns:
      table_blocks : list of Block(TABLE) with pipe-delimited text
      skip_zones   : {1-based page number → [(x0,y0,x1,y1), ...]}
                     Text extractor must skip these zones to avoid double-extraction.

    pages_filter: if provided, only process these 1-based page numbers
                  (used to restrict to DIGITAL pages only when routing per-page).
    """
    table_blocks: list[Block] = []
    skip_zones:   dict[int, list[tuple]] = {}

    with pdfplumber.open(path) as pdf:
        for page_num, page in enumerate(pdf.pages, start=1):

            if pages_filter is not None and page_num not in pages_filter:
                continue

            # Try line-based detection first (better for ruled tables like govt forms)
            try:
                tables     = page.extract_tables(Config.TABLE_SETTINGS_LINES)
                table_objs = page.find_tables(Config.TABLE_SETTINGS_LINES)
            except Exception:
                tables     = []
                table_objs = []

            # Fallback to text-alignment if no line-based tables found
            if not tables:
                try:
                    tables     = page.extract_tables(Config.TABLE_SETTINGS_TEXT)
                    table_objs = page.find_tables(Config.TABLE_SETTINGS_TEXT)
                except Exception:
                    tables     = []
                    table_objs = []

            # Register bounding boxes so text extractor skips them
            skip_zones[page_num] = []
            for tobj in table_objs:
                try:
                    skip_zones[page_num].append(tobj.bbox)
                except Exception:
                    pass

            for table in tables:
                rows = []
                for row in table:
                    cells = [str(c).strip() if c is not None else "" for c in row]
                    if any(cells):
                        rows.append(" | ".join(cells))
                if rows:
                    table_blocks.append(Block(
                        block_type=BlockType.TABLE,
                        text="\n".join(rows),
                        page=page_num,
                        source="digital",
                    ))

    log.info(f"Tables found (digital): {len(table_blocks)}")
    return table_blocks, skip_zones


# ─────────────────────────────────────────────────────────────────────────────
# STEP 2B — Text extraction via pdfminer (layout-aware)  — UNCHANGED
#
# pdfminer gives us:
#   - LTTextBox: a group of lines that belong together spatially
#   - LTChar: individual characters with font name + size
#
# We use font size relative to the page median to classify:
#   - Large font  → HEADING
#   - Normal font → TEXT or KV_BLOCK or LIST
#   - Small font  → CAPTION
#
# We skip any text whose bounding box overlaps a table zone (from Step 2A).
# ─────────────────────────────────────────────────────────────────────────────

def _dominant_font_size(textbox: LTTextBox) -> float:
    """Return the most common font size in this text box."""
    sizes: list[float] = []
    for line in textbox:
        if isinstance(line, LTTextLine):
            for ch in line:
                if isinstance(ch, LTChar) and ch.size > 0:
                    sizes.append(round(ch.size, 1))
    if not sizes:
        return 0.0
    # Most frequent size
    return max(set(sizes), key=sizes.count)


def _overlaps_table(box: tuple, zones: list[tuple], tolerance: float = 2.0) -> bool:
    """True if box overlaps any table zone (with small tolerance for PDF coord noise)."""
    x0, y0, x1, y1 = box
    for (tx0, ty0, tx1, ty1) in zones:
        if x0 < tx1 - tolerance and x1 > tx0 + tolerance \
           and y0 < ty1 - tolerance and y1 > ty0 + tolerance:
            return True
    return False


def _classify_text(text: str, font_size: float, median_size: float, x0: float) -> BlockType:
    """
    Classify a text block purely from visual signals — no keyword assumptions.
    Works on any document type, any language, any structure.
    """
    text = text.strip()
    lines = [l.strip() for l in text.splitlines() if l.strip()]

    if not lines:
        return BlockType.TEXT

    # Font-size signals
    if median_size > 0:
        ratio = font_size / median_size
        if ratio >= Config.HEADING_FONT_RATIO:
            return BlockType.HEADING
        if ratio <= Config.CAPTION_FONT_RATIO:
            return BlockType.CAPTION

    # Key-value heuristic: multiple short lines, each looks like "Label  Value"
    # Works for any language — just checks line count and avg word count
    if len(lines) >= 2:
        avg_words = sum(len(l.split()) for l in lines) / len(lines)
        if 2 <= avg_words <= 12 and all(len(l) < 120 for l in lines):
            # Check if lines have internal whitespace gaps (KV spacing)
            gap_lines = sum(1 for l in lines if re.search(r'\s{2,}', l))
            if gap_lines >= len(lines) * 0.5:
                return BlockType.KV_BLOCK

    # List heuristic: lines starting with bullet-like chars or numbers
    list_starters = sum(
        1 for l in lines
        if re.match(r'^[\-\•\*\u2022\u25cf\d]+[\.\)]\s', l)
    )
    if list_starters >= len(lines) * 0.5:
        return BlockType.LIST

    # Single short line with no sentence-ending punctuation mid-line → heading candidate
    if len(lines) == 1 and len(text) < 100 and not text.endswith(","):
        words = text.split()
        if 1 <= len(words) <= 10:
            return BlockType.HEADING

    return BlockType.TEXT


def extract_text_blocks(
    path: str,
    skip_zones: dict[int, list[tuple]],
    pages_filter: Optional[set[int]] = None,
) -> list[Block]:
    """
    Use pdfminer to extract all text boxes outside table zones.
    Classify each box by font size + content patterns.

    pages_filter: if provided, only process these 1-based page numbers
                  (used to restrict to DIGITAL pages only when routing per-page).
    """
    all_blocks: list[Block] = []

    for page_layout in extract_pages(path):
        if not isinstance(page_layout, LTPage):
            continue

        page_num = page_layout.pageid   # 1-based

        if pages_filter is not None and page_num not in pages_filter:
            continue

        zones    = skip_zones.get(page_num, [])

        # Collect all text boxes on this page with font sizes
        page_boxes: list[tuple[float, float, float, LTTextBox]] = []
        for element in page_layout:
            if not isinstance(element, LTTextBox):
                continue
            text = element.get_text().strip()
            if not text:
                continue
            fs = _dominant_font_size(element)
            page_boxes.append((element.x0, element.y0, fs, element))

        if not page_boxes:
            continue

        # Compute median font size for this page (used for heading detection)
        font_sizes = [fs for _, _, fs, _ in page_boxes if fs > 0]
        median_fs  = sorted(font_sizes)[len(font_sizes) // 2] if font_sizes else 10.0

        for x0, y0, fs, element in page_boxes:
            text = element.get_text().strip()
            bbox = (element.x0, element.y0, element.x1, element.y1)

            # Skip if this box is inside a table zone
            if _overlaps_table(bbox, zones):
                continue

            block_type = _classify_text(text, fs, median_fs, x0)

            all_blocks.append(Block(
                block_type=block_type,
                text=text,
                page=page_num,
                font_size=fs,
                x0=x0,
                y0=y0,
                source="digital",
            ))

    log.info(f"Text blocks found (digital): {len(all_blocks)}")
    return all_blocks


# ─────────────────────────────────────────────────────────────────────────────
# NEW — STEP 2C — OCR extraction for SCANNED / image pages
#
# Pipeline per page/image:
#   1. Preprocess with OpenCV (deskew, denoise, CLAHE contrast, threshold)
#   2. Detect ruled table grids via OpenCV line detection
#   3. Run pytesseract to get word-level text + bounding boxes
#   4. Words inside a detected table grid -> grouped into TABLE block
#      (pipe-delimited rows, same format as pdfplumber tables)
#   5. Words outside tables -> grouped into lines -> classified into
#      HEADING / TEXT / LIST / CAPTION / KeyValue using the SAME heuristics
#      as the digital path, but using bbox height as a font-size proxy.
# ─────────────────────────────────────────────────────────────────────────────

_TESSERACT_CONFIGURED = False

def _configure_tesseract():
    global _TESSERACT_CONFIGURED
    if _TESSERACT_CONFIGURED:
        return
    cmd = os.environ.get("TESSERACT_CMD")
    if cmd:
        pytesseract.pytesseract.tesseract_cmd = cmd
    elif os.name == "nt" and os.path.exists(Config.TESSERACT_CMD_WIN):
        pytesseract.pytesseract.tesseract_cmd = Config.TESSERACT_CMD_WIN
    _TESSERACT_CONFIGURED = True


def render_pdf_page_to_image(pdf_path: str, page_index_0based: int, dpi: int = Config.OCR_RENDER_DPI) -> np.ndarray:
    """Render a single PDF page to a BGR image using PyMuPDF (no poppler needed)."""
    doc = fitz.open(pdf_path)
    try:
        page = doc[page_index_0based]
        zoom = dpi / 72.0
        mat = fitz.Matrix(zoom, zoom)
        pix = page.get_pixmap(matrix=mat)
        img = np.frombuffer(pix.samples, dtype=np.uint8).reshape(pix.height, pix.width, pix.n)
        if pix.n == 4:
            img = cv2.cvtColor(img, cv2.COLOR_RGBA2BGR)
        elif pix.n == 1:
            img = cv2.cvtColor(img, cv2.COLOR_GRAY2BGR)
        else:
            img = cv2.cvtColor(img, cv2.COLOR_RGB2BGR)
        return img
    finally:
        doc.close()


def preprocess_image_for_ocr(img: np.ndarray) -> np.ndarray:
    """
    Clean up a scanned/photographed/xerox page before OCR.
    - Grayscale
    - Denoise (handles grainy/noisy xerox scans)
    - CLAHE contrast enhancement (fixes uneven xerox lighting)
    - Adaptive threshold (binarize for cleaner OCR on noisy backgrounds)
    - Deskew (rotate to correct small tilt)
    Returns a single-channel (grayscale) thresholded image.
    """
    gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY) if img.ndim == 3 else img.copy()

    gray = cv2.fastNlMeansDenoising(gray, h=10)

    clahe = cv2.createCLAHE(clipLimit=2.0, tileGridSize=(8, 8))
    gray = clahe.apply(gray)

    thresh = cv2.adaptiveThreshold(
        gray, 255, cv2.ADAPTIVE_THRESH_GAUSSIAN_C, cv2.THRESH_BINARY, 31, 15
    )

    deskewed = _deskew(thresh)
    return deskewed


def _deskew(img: np.ndarray) -> np.ndarray:
    """Detect skew angle via minAreaRect on dark pixels and rotate to correct it."""
    coords = np.column_stack(np.where(img < 255))
    if len(coords) < 50:
        return img  # not enough content to estimate skew

    angle = cv2.minAreaRect(coords)[-1]
    if angle < -45:
        angle = -(90 + angle)
    else:
        angle = -angle

    if abs(angle) < 0.5:
        return img  # negligible skew, skip rotation

    (h, w) = img.shape[:2]
    center = (w // 2, h // 2)
    M = cv2.getRotationMatrix2D(center, angle, 1.0)
    rotated = cv2.warpAffine(
        img, M, (w, h), flags=cv2.INTER_CUBIC, borderMode=cv2.BORDER_REPLICATE
    )
    return rotated


def _detect_table_grids(thresh_img: np.ndarray) -> list[tuple[int, int, int, int]]:
    """
    Detect ruled table regions on a preprocessed (binary) image using
    OpenCV morphological line detection. Returns bounding boxes
    (x0, y0, x1, y1) in pixel coordinates of regions containing a grid
    of horizontal + vertical lines (i.e. a table).
    """
    h, w = thresh_img.shape[:2]
    inv = cv2.bitwise_not(thresh_img)  # lines become white on black

    horiz_len = max(int(w * Config.TABLE_LINE_MIN_LENGTH_RATIO), 20)
    vert_len  = max(int(h * Config.TABLE_LINE_MIN_LENGTH_RATIO), 20)

    horiz_kernel = cv2.getStructuringElement(cv2.MORPH_RECT, (horiz_len, 1))
    vert_kernel  = cv2.getStructuringElement(cv2.MORPH_RECT, (1, vert_len))

    horiz_lines = cv2.morphologyEx(inv, cv2.MORPH_OPEN, horiz_kernel, iterations=1)
    vert_lines  = cv2.morphologyEx(inv, cv2.MORPH_OPEN, vert_kernel, iterations=1)

    grid = cv2.bitwise_or(horiz_lines, vert_lines)
    grid = cv2.dilate(grid, np.ones((3, 3), np.uint8), iterations=1)

    contours, _ = cv2.findContours(grid, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)

    boxes = []
    for c in contours:
        x, y, cw, ch = cv2.boundingRect(c)
        # Require a reasonably sized region (avoid tiny noise contours)
        if cw > w * 0.15 and ch > h * 0.03:
            boxes.append((x, y, x + cw, y + ch))

    return boxes


def _ocr_words(img_for_ocr: np.ndarray) -> list[dict]:
    """
    Run pytesseract image_to_data and return a list of word dicts:
    {text, conf, x0, y0, x1, y1}  (pixel coordinates, y0=top of image)
    """
    _configure_tesseract()
    data = pytesseract.image_to_data(
        img_for_ocr,
        lang=Config.OCR_LANG,
        output_type=pytesseract.Output.DICT,
        config="--psm 3",
    )

    words = []
    n = len(data["text"])
    for i in range(n):
        txt = data["text"][i].strip()
        if not txt:
            continue
        try:
            conf = float(data["conf"][i])
        except (ValueError, TypeError):
            conf = -1.0
        if conf < Config.OCR_MIN_WORD_CONFIDENCE:
            continue
        x, y, w, h = data["left"][i], data["top"][i], data["width"][i], data["height"][i]
        words.append({
            "text": txt, "conf": conf,
            "x0": x, "y0": y, "x1": x + w, "y1": y + h,
        })
    return words


def _group_words_into_lines(words: list[dict], y_tolerance: int = 8) -> list[dict]:
    """
    Group OCR words into lines based on vertical (y) proximity, then sort
    each line's words left-to-right. Returns list of:
    {text, x0, y0, x1, y1, height}
    """
    if not words:
        return []

    words_sorted = sorted(words, key=lambda w: (w["y0"], w["x0"]))

    lines: list[list[dict]] = []
    for w in words_sorted:
        placed = False
        for line in lines:
            line_y0 = sum(x["y0"] for x in line) / len(line)
            if abs(w["y0"] - line_y0) <= y_tolerance:
                line.append(w)
                placed = True
                break
        if not placed:
            lines.append([w])

    grouped = []
    for line in lines:
        line_sorted = sorted(line, key=lambda x: x["x0"])
        text = " ".join(x["text"] for x in line_sorted)
        x0 = min(x["x0"] for x in line_sorted)
        y0 = min(x["y0"] for x in line_sorted)
        x1 = max(x["x1"] for x in line_sorted)
        y1 = max(x["y1"] for x in line_sorted)
        grouped.append({
            "text": text, "x0": x0, "y0": y0, "x1": x1, "y1": y1,
            "height": max(y1 - y0, 1),
        })

    return grouped


def _word_in_box(word: dict, box: tuple[int, int, int, int], tolerance: int = 5) -> bool:
    bx0, by0, bx1, by1 = box
    cx = (word["x0"] + word["x1"]) / 2
    cy = (word["y0"] + word["y1"]) / 2
    return (bx0 - tolerance) <= cx <= (bx1 + tolerance) and (by0 - tolerance) <= cy <= (by1 + tolerance)


def _build_table_block_from_words(words: list[dict], page_num: int, table_box: tuple[int, int, int, int]) -> Optional[Block]:
    """
    Group words inside a table region into rows (by y) and columns (by x
    clustering), producing a pipe-delimited TABLE block — same text format
    as the digital pdfplumber path.
    """
    if not words:
        return None

    lines = _group_words_into_lines(words, y_tolerance=10)
    if len(lines) < Config.TABLE_MIN_ROWS:
        return None

    # Estimate column boundaries by clustering x0 positions of all words
    all_x0 = sorted(w["x0"] for w in words)
    col_breaks = [all_x0[0]]
    gap_threshold = 30  # pixels — gap larger than this starts a new column
    for x in all_x0[1:]:
        if x - col_breaks[-1] > gap_threshold:
            col_breaks.append(x)

    if len(col_breaks) < Config.TABLE_MIN_COLS:
        # Not enough column structure detected — treat as plain text instead
        return None

    rows_text = []
    for line in lines:
        line_words = sorted(
            [w for w in words if abs(w["y0"] - line["y0"]) <= 10 or
             (w["y0"] >= line["y0"] - 10 and w["y1"] <= line["y1"] + 10)],
            key=lambda w: w["x0"]
        )
        if not line_words:
            continue

        # Assign each word to nearest column break
        cols: dict[int, list[str]] = {}
        for w in line_words:
            col_idx = max(i for i, cb in enumerate(col_breaks) if cb <= w["x0"] + 1)
            cols.setdefault(col_idx, []).append(w["text"])

        row_cells = []
        for i in range(len(col_breaks)):
            row_cells.append(" ".join(cols.get(i, [])).strip())
        if any(row_cells):
            rows_text.append(" | ".join(row_cells))

    if len(rows_text) < Config.TABLE_MIN_ROWS:
        return None

    bx0, by0, bx1, by1 = table_box
    return Block(
        block_type=BlockType.TABLE,
        text="\n".join(rows_text),
        page=page_num,
        x0=float(bx0),
        y0=-float(by0),  # negate so sort order matches pdfminer convention (top=high y0)
        font_size=0.0,
        source="ocr",
    )


def _classify_ocr_line(text: str, height: float, median_height: float) -> BlockType:
    """
    Same heuristics as _classify_text, adapted for OCR output:
    uses bbox height as a proxy for font size.
    """
    text = text.strip()
    if not text:
        return BlockType.TEXT

    if median_height > 0:
        ratio = height / median_height
        if ratio >= Config.OCR_HEADING_HEIGHT_RATIO:
            return BlockType.HEADING
        if ratio <= Config.OCR_CAPTION_HEIGHT_RATIO:
            return BlockType.CAPTION

    # List heuristic
    if re.match(r'^[\-\•\*\u2022\u25cf\d]+[\.\)]\s', text):
        return BlockType.LIST

    # Key-value heuristic — "Label: Value" or "Label - Value" patterns common
    # in OCR'd government forms (Aadhaar, PAN, bank letters, etc.)
    if re.match(r'^[A-Za-z\u0900-\u097F\u0B80-\u0BFF\u0C00-\u0C7F0-9 ./]{2,40}[:\-]\s*\S+', text):
        return BlockType.KV_BLOCK

    # Single short line -> heading candidate
    if len(text) < 100 and not text.endswith(","):
        words = text.split()
        if 1 <= len(words) <= 10:
            return BlockType.HEADING

    return BlockType.TEXT


def ocr_image_to_blocks(img: np.ndarray, page_num: int) -> list[Block]:
    """
    Full OCR pipeline for one image (scanned PDF page or standalone photo):
    preprocess -> detect tables -> OCR words -> classify lines -> Block list.
    """
    pre = preprocess_image_for_ocr(img)

    # 1. Detect table regions on the preprocessed (binary) image
    table_boxes = _detect_table_grids(pre)

    # 2. OCR the whole page once
    words = _ocr_words(pre)
    if not words:
        log.info(f"OCR found no text on page {page_num}")
        return []

    blocks: list[Block] = []
    used_word_ids: set[int] = set()

    # 3. Build TABLE blocks from words inside detected table regions
    for box in table_boxes:
        table_words = [w for w in words if _word_in_box(w, box)]
        if not table_words:
            continue
        tblock = _build_table_block_from_words(table_words, page_num, box)
        if tblock:
            blocks.append(tblock)
            for w in table_words:
                used_word_ids.add(id(w))

    # 4. Remaining words -> grouped into lines -> classified
    remaining = [w for w in words if id(w) not in used_word_ids]
    lines = _group_words_into_lines(remaining)

    if lines:
        heights = sorted(l["height"] for l in lines)
        median_h = heights[len(heights) // 2]

        for line in lines:
            block_type = _classify_ocr_line(line["text"], line["height"], median_h)
            blocks.append(Block(
                block_type=block_type,
                text=line["text"],
                page=page_num,
                font_size=float(line["height"]),
                x0=float(line["x0"]),
                y0=-float(line["y0"]),  # negate so sort order matches pdfminer convention
                source="ocr",
            ))

    log.info(f"OCR blocks found on page {page_num}: {len(blocks)} "
             f"({len(table_boxes)} table region(s))")
    return blocks


# ─────────────────────────────────────────────────────────────────────────────
# STEP 2D — Merge and sort all blocks into reading order  — UNCHANGED
#
# PDF pages have blocks in arbitrary order in the file.
# Sort by (page, y_descending, x_ascending) to match natural reading order.
# y is inverted because PDF y=0 is bottom of page (and we negate OCR y to match).
# ─────────────────────────────────────────────────────────────────────────────

def merge_and_sort(
    text_blocks: list[Block],
    table_blocks: list[Block],
) -> list[Block]:
    """
    Combine text and table blocks, sort into reading order.
    Tables are inserted at the y-position where they were found.
    """
    all_blocks = text_blocks + table_blocks
    all_blocks.sort(key=lambda b: (b.page, -b.y0, b.x0))
    return all_blocks


# ─────────────────────────────────────────────────────────────────────────────
# STEP 3 — Semantic chunking  — UNCHANGED (except source tracking added)
#
# Strategy:
#   1. Walk blocks in reading order
#   2. HEADING → flush current buffer, start new chunk
#   3. TABLE   → flush current buffer, TABLE becomes its own chunk, resume
#   4. TEXT/KV/LIST/CAPTION → accumulate in buffer
#   5. If buffer exceeds MAX_TOKENS → flush and continue
#
# This means every chunk is semantically bounded by document structure,
# not by an arbitrary character window.
# ─────────────────────────────────────────────────────────────────────────────

def build_chunks(blocks: list[Block]) -> list[Chunk]:
    results: list[Chunk] = []
    order   = 0

    buffer:          list[Block] = []
    current_section: Optional[str] = None

    def flush(buf: list[Block]) -> None:
        nonlocal order
        if not buf:
            return
        text = "\n\n".join(b.text for b in buf).strip()
        tc   = token_count(text)
        if tc < Config.MIN_TOKENS_TO_KEEP:
            return

        # Collect section title: last HEADING in buffer, or current_section
        headings = [b.text for b in buf if b.block_type == BlockType.HEADING]
        section  = headings[-1] if headings else current_section

        # Determine chunk source: digital / ocr / mixed
        sources = {b.source for b in buf}
        if len(sources) == 1:
            chunk_source = sources.pop()
        else:
            chunk_source = "mixed"

        cid = str(uuid.uuid4())
        results.append(Chunk(
            chunk_id=cid,
            chunk_order=order,
            text=text,
            metadata=ChunkMeta(
                chunk_id=cid,
                chunk_order=order,
                section=section,
                block_types=list(dict.fromkeys(b.block_type.value for b in buf)),
                pages=sorted({b.page for b in buf}),
                has_table=any(b.block_type == BlockType.TABLE for b in buf),
                token_count=tc,
                char_count=len(text),
                source=chunk_source,
            ),
        ))
        order += 1

    for block in blocks:

        # TABLE always gets its own isolated chunk
        if block.block_type == BlockType.TABLE:
            flush(buffer)
            buffer = []
            flush([block])
            continue

        # HEADING → new chunk boundary
        if block.block_type == BlockType.HEADING:
            flush(buffer)
            buffer = [block]
            current_section = block.text
            continue

        # Check token ceiling before accumulating
        current_tokens = token_count("\n\n".join(b.text for b in buffer)) if buffer else 0
        new_tokens     = token_count(block.text)

        if current_tokens + new_tokens > Config.MAX_TOKENS_PER_CHUNK and buffer:
            flush(buffer)
            buffer = [block]
        else:
            buffer.append(block)

    flush(buffer)

    log.info(f"Chunks built: {len(results)}")
    return results


# ─────────────────────────────────────────────────────────────────────────────
# MAIN EXTRACTION PIPELINE — now with per-page routing
# ─────────────────────────────────────────────────────────────────────────────

def process_pdf(path: str) -> tuple[DocumentType, str, list[Block], list[Chunk]]:
    # 1. Detect overall doc type (for reporting/strategy string only)
    doc_type, coverage = detect_type(path)

    # 2. Per-page routing: which pages are digital vs need OCR
    page_is_digital = detect_page_text_layers(path)
    total_pages = len(page_is_digital)
    if total_pages == 0:
        raise ValueError("PDF has no pages.")

    digital_pages = {i + 1 for i, d in enumerate(page_is_digital) if d}
    ocr_pages     = {i + 1 for i, d in enumerate(page_is_digital) if not d}

    log.info(f"Routing: {len(digital_pages)} digital page(s), "
             f"{len(ocr_pages)} OCR page(s) of {total_pages} total")

    text_blocks: list[Block] = []
    table_blocks: list[Block] = []

    # 3. DIGITAL pages — ORIGINAL pdfplumber + pdfminer pipeline (unchanged)
    if digital_pages:
        d_table_blocks, skip_zones = extract_tables(path, pages_filter=digital_pages)
        d_text_blocks = extract_text_blocks(path, skip_zones, pages_filter=digital_pages)
        table_blocks.extend(d_table_blocks)
        text_blocks.extend(d_text_blocks)

    # 4. OCR pages — NEW preprocessing + pytesseract pipeline
    for page_num in sorted(ocr_pages):
        img = render_pdf_page_to_image(path, page_num - 1)
        ocr_blocks = ocr_image_to_blocks(img, page_num)
        for b in ocr_blocks:
            if b.block_type == BlockType.TABLE:
                table_blocks.append(b)
            else:
                text_blocks.append(b)

    # 5. Merge into reading order (unchanged)
    all_blocks = merge_and_sort(text_blocks, table_blocks)

    # 6. Build semantic chunks (unchanged)
    chunks = build_chunks(all_blocks)

    strategy = (
        f"per-page routing | overall_type={doc_type.value} coverage={coverage:.0%} | "
        f"digital_pages={len(digital_pages)} ocr_pages={len(ocr_pages)} "
        f"total_pages={total_pages}"
    )

    return doc_type, strategy, all_blocks, chunks


def process_image(path: str) -> tuple[DocumentType, str, list[Block], list[Chunk]]:
    """
    NEW: standalone image input (.jpg/.png/.tiff/.bmp) — e.g. a phone photo
    of a form, a scanned xerox page saved as an image, etc.
    """
    img = cv2.imread(path)
    if img is None:
        raise ValueError(f"Could not read image file: {path}")

    blocks = ocr_image_to_blocks(img, page_num=1)

    table_blocks = [b for b in blocks if b.block_type == BlockType.TABLE]
    text_blocks  = [b for b in blocks if b.block_type != BlockType.TABLE]

    all_blocks = merge_and_sort(text_blocks, table_blocks)
    chunks = build_chunks(all_blocks)

    strategy = f"image input | OCR (pytesseract + OpenCV preprocessing)"

    return DocumentType.SCANNED, strategy, all_blocks, chunks


def process_document(path: str) -> tuple[DocumentType, str, list[Block], list[Chunk]]:
    """
    NEW: top-level dispatcher — routes to process_pdf or process_image
    based on file extension.
    """
    ext = path.lower().rsplit(".", 1)[-1] if "." in path else ""
    if ext == "pdf":
        return process_pdf(path)
    elif ext in ("jpg", "jpeg", "png", "tiff", "tif", "bmp"):
        return process_image(path)
    else:
        raise ValueError(f"Unsupported file type: .{ext}")


# ─────────────────────────────────────────────────────────────────────────────
# FASTAPI
# ─────────────────────────────────────────────────────────────────────────────

app = FastAPI(
    title="GeoAccess AI Service",
    version="5.0",
    description=(
        "Structure-agnostic document extraction. "
        "Digital PDFs use pdfplumber + pdfminer (tables, KV pairs, headings, "
        "paragraphs). Scanned PDFs, photos, xerox copies, and images use "
        "OpenCV preprocessing + pytesseract OCR with table-grid detection. "
        "Per-page routing handles mixed documents automatically."
    ),
)


@app.get("/")
def home():
    return {"message": "GeoAccess AI Service", "version": "5.0"}


@app.get("/health")
def health():
    return {"status": "healthy"}


SUPPORTED_EXTENSIONS = (".pdf", ".jpg", ".jpeg", ".png", ".tiff", ".tif", ".bmp")


@app.post("/process-document", response_model=Result)
async def process_document_endpoint(file: UploadFile = File(...)):
    fname_lower = file.filename.lower()
    if not fname_lower.endswith(SUPPORTED_EXTENSIONS):
        raise HTTPException(400, f"Unsupported file type. Supported: {SUPPORTED_EXTENSIONS}")

    suffix = "." + fname_lower.rsplit(".", 1)[-1]
    tmp_path: Optional[str] = None
    try:
        with tempfile.NamedTemporaryFile(delete=False, suffix=suffix) as tmp:
            tmp.write(await file.read())
            tmp_path = tmp.name

        doc_id = str(uuid.uuid4())
        log.info(f"Processing {file.filename} → {doc_id}")

        doc_type, strategy, blocks, chunks = process_document(tmp_path)

        return Result(
            file_name=file.filename,
            doc_id=doc_id,
            doc_type=doc_type,
            strategy=strategy,
            block_count=len(blocks),
            chunk_count=len(chunks),
            chunks=chunks,
        )

    except HTTPException:
        raise
    except ValueError as ve:
        raise HTTPException(422, str(ve))
    except Exception as ex:
        log.exception(f"Failed: {file.filename}")
        raise HTTPException(500, str(ex))
    finally:
        if tmp_path and os.path.exists(tmp_path):
            os.remove(tmp_path)


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
def generate_embedding(text: str):

    # api_key = os.getenv("AQ.Ab8RN6JF9lQVgcEB-c1OhZy_p299YQv0xUMDH3uZ2B3muCPnWQ")
    api_key="AQ.Ab8RN6JF9lQVgcEB-c1OhZy_p299YQv0xUMDH3uZ2B3muCPnWQ";

    response = requests.post(
        f"https://generativelanguage.googleapis.com/v1beta/models/gemini-embedding-001:embedContent?key={api_key}",
        json={
            "model": "models/gemini-embedding-001",
            "content": {
                "parts": [
                    {
                        "text": text
                    }
                ]
            }
        },
        timeout=120
    )

    response.raise_for_status()

    return response.json()["embedding"]["values"]

# @app.get("/test-embedding")
# def test_embedding():

#     embedding = generate_embedding("Hello World")

#     return {
#         "dimension": len(embedding),
#         "sample": embedding[:5]
#     }

def build_embedding_text(
    document_type: str,
    section: str | None,
    chunk_text: str
):
    return f"""
Document Type: {document_type}

Section: {section or "General"}

{chunk_text}
"""


@app.post(
    "/process-document-with-embeddings",
    response_model=EmbeddingResult
)
async def process_document_with_embeddings(
    file: UploadFile = File(...)
):
    fname_lower = file.filename.lower()
    if not fname_lower.endswith(SUPPORTED_EXTENSIONS):
        raise HTTPException(
            status_code=400,
            detail=f"Unsupported file type. Supported: {SUPPORTED_EXTENSIONS}"
        )

    suffix = "." + fname_lower.rsplit(".", 1)[-1]
    tmp_path = None

    try:
        with tempfile.NamedTemporaryFile(
            delete=False,
            suffix=suffix
        ) as tmp:

            tmp.write(await file.read())
            tmp_path = tmp.name

        doc_id = str(uuid.uuid4())

        doc_type, strategy, blocks, chunks = process_document(
            tmp_path
        )

        embedded_chunks = []

        for chunk in chunks:

            embedding_text = build_embedding_text(
                doc_type.value,
                chunk.metadata.section,
                chunk.text
            )

            embedding = generate_embedding(
                embedding_text
            )

            embedded_chunks.append(
                EmbeddedChunk(
                    chunk_id=chunk.chunk_id,
                    chunk_order=chunk.chunk_order,
                    text=chunk.text,
                    embedding=embedding,
                    metadata=chunk.metadata
                )
            )

        return EmbeddingResult(
            file_name=file.filename,
            doc_id=doc_id,
            doc_type=doc_type,
            strategy=strategy,
            block_count=len(blocks),
            chunk_count=len(embedded_chunks),
            chunks=embedded_chunks
        )

    except HTTPException:
        raise

    except ValueError as ve:
        raise HTTPException(422, str(ve))

    except Exception as ex:
        log.exception(ex)

        raise HTTPException(
            status_code=500,
            detail=str(ex)
        )

    finally:
        if tmp_path and os.path.exists(tmp_path):
            os.remove(tmp_path)