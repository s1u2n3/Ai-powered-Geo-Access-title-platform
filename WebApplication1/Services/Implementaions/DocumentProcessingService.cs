using Amazon.S3.Model;
using Docnet.Core;
using Docnet.Core.Models;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using Tesseract;
using WebApplication1.Repositories.Interfaces.DocumentProcessing;
using WebApplication1.Services.Implementaions;
using WebApplication1.Services.Interfaces;
using UglyToad.PdfPig;




public class DocumentProcessingService
    : IDocumentProcessingService
{
    private readonly S3Service _s3Service;

    private readonly
        IDocumentChunkRepository
            _chunkRepository;
    private readonly EmbeddingsService _embeddingsService;

    public DocumentProcessingService(S3Service s3Service,IDocumentChunkRepository chunkRepository,EmbeddingsService embeddingsService)
    {
        _s3Service = s3Service;

        _chunkRepository = chunkRepository;
        _embeddingsService= embeddingsService;
    }

    public async Task
        ProcessParcelDocumentsAsync(
            List<S3Object> documents)
    {
        foreach (var document in documents)
        {
            var stream =
                await _s3Service
                    .GetDocumentStreamAsync(
                        document.Key);

            var extractedText =
                await ExtractTextAsync(stream);

            var chunks =
                CreateChunks(
                    extractedText,
                    document.Key);

            await SaveChunksAsync(chunks);
        }
    }

    //using pdg gig and ocr fallback//
    private async Task<string> ExtractTextAsync(Stream stream)
    {
        using var memoryStream =
            new MemoryStream();

        await stream.CopyToAsync(memoryStream);

        var pdfBytes =
            memoryStream.ToArray();

        // STEP 1
        // Try extracting searchable PDF text

        var pdfText =
            TryExtractPdfText(pdfBytes);

        pdfText =
            CleanText(pdfText);

        if (!string.IsNullOrWhiteSpace(pdfText)
            && pdfText.Length > 100)
        {
            Console.WriteLine(
                $"PdfPig extracted {pdfText.Length} chars");

            return pdfText;
        }

        // STEP 2
        // OCR fallback

        Console.WriteLine(
            "PdfPig found little text. Running OCR...");

        var ocrText =
            ExtractUsingOcr(pdfBytes);

        ocrText =
            CleanText(ocrText);

        Console.WriteLine(
            $"OCR extracted {ocrText.Length} chars");

        return ocrText;
    }

    private string TryExtractPdfText(
        byte[] pdfBytes)
    {
        try
        {
            using var pdf =
                PdfDocument.Open(pdfBytes);

            var text =
                new StringBuilder();

            foreach (var page in pdf.GetPages())
            {
                text.AppendLine(page.Text);
            }

            return text.ToString();
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"PdfPig Error: {ex.Message}");

            return string.Empty;
        }
    }

    private string ExtractUsingOcr(
        byte[] pdfBytes)
    {
        var result =
            new StringBuilder();

        using var docReader =
            DocLib.Instance.GetDocReader(
                pdfBytes,
                new PageDimensions(
                    2480,
                    3508));

        int pageCount =
            docReader.GetPageCount();

        using var engine =
            new TesseractEngine(
                @"./tessdata",
                "eng",
                EngineMode.Default);

        for (int i = 0; i < pageCount; i++)
        {
            using var pageReader =
                docReader.GetPageReader(i);

            var rawBytes =
                pageReader.GetImage();

            int width =
                pageReader.GetPageWidth();

            int height =
                pageReader.GetPageHeight();

            using var bitmap =
                new Bitmap(
                    width,
                    height,
                    PixelFormat.Format32bppArgb);

            var bitmapData =
                bitmap.LockBits(
                    new Rectangle(
                        0,
                        0,
                        width,
                        height),
                    ImageLockMode.WriteOnly,
                    bitmap.PixelFormat);

            System.Runtime.InteropServices.Marshal.Copy(
                rawBytes,
                0,
                bitmapData.Scan0,
                rawBytes.Length);

            bitmap.UnlockBits(bitmapData);

            using var pix =
                PixConverter.ToPix(bitmap);

            using var page =
                engine.Process(pix);

            result.AppendLine(
                page.GetText());
        }

        return result.ToString();
    }

    private string CleanText(
        string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        text =
            text.Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("\t", " ");

        text =
            System.Text.RegularExpressions.Regex
                .Replace(
                    text,
                    @"\s+",
                    " ");

        return text.Trim();
    }


    //private async Task<string>
    //  ExtractTextAsync(Stream stream)
    //{
    //    using var memoryStream =
    //        new MemoryStream();

    //    await stream.CopyToAsync(memoryStream);

    //    var pdfBytes =
    //        memoryStream.ToArray();

    //    var result =
    //        new StringBuilder();

    //    using var docReader =
    //        DocLib.Instance.GetDocReader(
    //            pdfBytes,
    //            new PageDimensions(3508, 4961));

    //    int pageCount =
    //        docReader.GetPageCount();

    //    using var engine =
    //        new TesseractEngine(
    //            @"./tessdata",
    //            "eng",
    //            EngineMode.Default);

    //    for (int i = 0; i < pageCount; i++)
    //    {
    //        using var pageReader =
    //            docReader.GetPageReader(i);

    //        var rawBytes =
    //            pageReader.GetImage();

    //        int width =
    //            pageReader.GetPageWidth();

    //        int height =
    //            pageReader.GetPageHeight();

    //        using var bitmap =
    //            new Bitmap(
    //                width,
    //                height,
    //                PixelFormat.Format32bppArgb);

    //        var bitmapData =
    //            bitmap.LockBits(
    //                new Rectangle(0, 0, width, height),
    //                ImageLockMode.WriteOnly,
    //                bitmap.PixelFormat);

    //        System.Runtime.InteropServices.Marshal.Copy(
    //            rawBytes,
    //            0,
    //            bitmapData.Scan0,
    //            rawBytes.Length);

    //        bitmap.UnlockBits(bitmapData);

    //        using var pix =
    //            PixConverter.ToPix(bitmap);

    //        using var page =
    //            engine.Process(pix);

    //        result.AppendLine(
    //            page.GetText());
    //    }

    //    return result.ToString();
    //}

    //private async Task<string>
    //  ExtractTextAsync(Stream stream)
    //{
    //    using var memoryStream =
    //        new MemoryStream();

    //    await stream.CopyToAsync(memoryStream);

    //    memoryStream.Position = 0;

    //    using var pdf =
    //        PdfDocument.Open(memoryStream);

    //    var text =
    //        new StringBuilder();

    //    foreach (var page in pdf.GetPages())
    //    {
    //        text.AppendLine(
    //            page.Text);
    //    }

    //    return text.ToString();
    //}

    //private List<DocumentChunk>
    //    CreateChunks(
    //        string extractedText,
    //        string s3Key)
    //{
    //    var splitChunks =
    //        extractedText.Split(
    //            '.',
    //            StringSplitOptions
    //                .RemoveEmptyEntries);

    //    var result =
    //        new List<DocumentChunk>();

    //    var order = 0;

    //    foreach (var chunk in splitChunks)
    //    {
    //        result.Add(
    //            new DocumentChunk
    //            {
    //                Id = Guid.NewGuid(),

    //                DocumentId = s3Key,

    //                ParcelId =
    //                    ExtractParcelId(s3Key),

    //                ChunkText = chunk.Trim(),

    //                ChunkType = "General",

    //                ChunkOrder = order++
    //            });
    //    }

    //    return result;
    ////}
    private List<DocumentChunk>
    CreateChunks(
        string extractedText,
        string s3Key)
    {
        var result =
            new List<DocumentChunk>();

        var cleanedText =
            extractedText
                .Replace("\r", " ")
                .Replace("\n", " ");

        cleanedText =
            System.Text.RegularExpressions.Regex
                .Replace(cleanedText, @"\s+", " ");

        int chunkSize = 1200;

        int overlap = 200;

        int order = 0;

        for (int i = 0;
             i < cleanedText.Length;
             i += (chunkSize - overlap))
        {
            int length =
                Math.Min(
                    chunkSize,
                    cleanedText.Length - i);

            var chunk =
                cleanedText.Substring(i, length);

            if (string.IsNullOrWhiteSpace(chunk))
                continue;

            result.Add(
                new DocumentChunk
                {
                    Id = Guid.NewGuid(),

                    DocumentId = s3Key,

                    ParcelId =
                        ExtractParcelId(s3Key),

                    ChunkText = chunk,

                    ChunkType = "General",

                    ChunkOrder = order++
                });
        }

        return result;
    }

    private async Task
        SaveChunksAsync(
            List<DocumentChunk> chunks)
    {
        foreach (var chunk in chunks)
        {
            chunk.Embedding = await _embeddingsService.GenerateEmbeddingAsync(chunk.ChunkText);
        }

        await _chunkRepository
            .SaveChunksAsync(chunks);
    }
    private string ExtractParcelId(
        string s3Key)
    {
        var segments =
            s3Key.Split('/');

        return segments[1]
            .Replace("parcel-", "P-")
            .ToUpper();
    }
}