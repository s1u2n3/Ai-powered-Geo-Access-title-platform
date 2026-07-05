using Amazon.S3.Model;
using Docnet.Core;
using Docnet.Core.Models;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Tesseract;
using UglyToad.PdfPig;
using WebApplication1.Dto.DocumentJobs;
using WebApplication1.Dto.FastApiResponse;
using WebApplication1.entites.AwsBackgroundJobs;
using WebApplication1.Repositories.Interfaces.DocumentProcessing;
using WebApplication1.Services.Implementaions;
using WebApplication1.Services.Interfaces;




public class DocumentProcessingService
    : IDocumentProcessingService
{
    private readonly S3Service _s3Service;
    private readonly HttpClient _httpClient;
    private readonly PostgresDbContext _postgresdb;
    private readonly IQueueService _queueService;
    private readonly IConfiguration _configuration;

    private readonly
        IDocumentChunkRepository
            _chunkRepository;
    private readonly EmbeddingsService _embeddingsService;

    public DocumentProcessingService(S3Service s3Service,IConfiguration configuration, IDocumentChunkRepository chunkRepository, EmbeddingsService embeddingsService, HttpClient httpClient,PostgresDbContext postgresDb, IQueueService queueService)
    {
        _s3Service = s3Service;
        _chunkRepository = chunkRepository;
        _embeddingsService = embeddingsService;
        _httpClient = httpClient;
        _postgresdb = postgresDb;
        _queueService = queueService;
        _configuration = configuration;        
    }


    public async Task<Guid> CreateDocumentProcessingJobAsync(string parcelId){

        var documents = await _s3Service.GetParcelDocumentsAsync(parcelId);

        var job = new DocumentProcessingJob
        {
            Id = Guid.NewGuid(),
            ParcelId = parcelId,
            Status = "Pending",
            TotalDocuments = documents.Count,
            ProcessedDocuments = 0,
            CreatedAt = DateTime.UtcNow
        };
        await  _postgresdb.DocumentProcessingJob.AddAsync(job);

        foreach (var document in documents)
        {
            Console.WriteLine($"Creating item for {document.Key}");
            var item = new DocumentProcessingItem
            {
                Id = Guid.NewGuid(),
                DocumentProcessingJobId = job.Id,
                DocumentName =
                     Path.GetFileName(document.Key),
                S3Key = document.Key,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            await _postgresdb.DocumentProcessingItem.AddAsync(item);
            await _postgresdb.SaveChangesAsync();
            await _queueService
           .SendMessageAsync(
               new DocumentProcessingMessage
               {
                   JobId = job.Id,
                   ItemId = item.Id,
                   ParcelId = parcelId,
                   S3Key = document.Key
               });
            Console.WriteLine($"SQS sent for {document.Key}");

        }
            
            return job.Id;       
    }
    public async Task ProcessSingleParcelDocument(string S3Key)
    {
        var stream = await _s3Service.GetDocumentStreamAsync(S3Key);
        var chunks = await ProcessDocumentViaFastApi(stream, S3Key);
        await SaveChunksAsync(chunks);
    }

    public async Task<DocumentProcessingItem>GetDocumentProcessingItem(Guid itemId)
    {
        return await _postgresdb.DocumentProcessingItem.FindAsync(itemId);

    }
    public async Task MarkProcessingAsync(Guid itemId)
    {
        var item = await _postgresdb.DocumentProcessingItem.FindAsync(itemId);

        if (item == null)
            return;
        item.Status = "Processing";
        await _postgresdb.SaveChangesAsync();

    }

    public async Task MarkCompletedAsync(Guid itemId)
    {
        var item = await _postgresdb.DocumentProcessingItem.FindAsync(itemId);

        if (item == null)
            return;
        item.Status = "Completed";
        item.CompletedAt = DateTime.UtcNow;
        await _postgresdb.SaveChangesAsync();
    }
    public async Task MarkFailedAsync(Guid itemId ,string error)
    {
        var item = await _postgresdb.DocumentProcessingItem.FindAsync(itemId);
        if (item == null)
        
            return;
            item.Status = "Failed";
            item.ErrorMessage = error;

        await _postgresdb.SaveChangesAsync();
    }
    public async Task UpdateJobProgressAsync(Guid JobId)
    {
        var job = await _postgresdb.DocumentProcessingJob.FindAsync(JobId);
        if (job == null)
            return;
        job.ProcessedDocuments++;
        if(job.ProcessedDocuments >= job.TotalDocuments)
        {
            job.Status = "Completed";
            job.CompletedAt = DateTime.UtcNow;
        }
        await _postgresdb.SaveChangesAsync();
    }
    //public async Task
    //    ProcessParcelDocumentsAsync(
    //        List<S3Object> documents)
    //{
    //    foreach (var document in documents)
    //    {
    //        var stream =
    //            await _s3Service
    //                .GetDocumentStreamAsync(
    //                    document.Key);

    //        var chunks =
    //      await ProcessDocumentViaFastApi(
    //       stream,
    //       document.Key);


    //        await SaveChunksAsync(chunks);
    //    }
    //}

    //call fast api for chunking and embeddings//
    private async Task<List<DocumentChunk>>
    ProcessDocumentViaFastApi(
        Stream pdfStream,
        string s3Key)
    {

        var fastapiUrl = _configuration["FastApi:url"];
        using var form =
            new MultipartFormDataContent();

        form.Add(
            new StreamContent(pdfStream),
            "file",
            Path.GetFileName(s3Key));

         var response =
            await _httpClient.PostAsync(
                fastapiUrl,
                form);

        response.EnsureSuccessStatusCode();

        var json =
            await response.Content
                .ReadAsStringAsync();

        var result =
            JsonSerializer.Deserialize<
                FastApiResponse>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        return result.Chunks
            .Select(x =>
                new DocumentChunk
                {
                    Id = Guid.NewGuid(),

                    DocumentId = s3Key,

                    ParcelId =
                        ExtractParcelId(s3Key),

                    ChunkText = x.Text,

                    ChunkOrder =
                        x.ChunkOrder,

                    ChunkType =
                        x.Metadata?.Section
                        ?? "General",

                    Embedding =new Pgvector.Vector(x.Embedding)
                })
            .ToList();
    }



//    //using pdg gig and ocr fallback//
//    private async Task<string> ExtractTextAsync(Stream stream)
//    {
//        using var memoryStream =
//            new MemoryStream();

//        await stream.CopyToAsync(memoryStream);

//        var pdfBytes =
//            memoryStream.ToArray();

//        // STEP 1
//        // Try extracting searchable PDF text

//        var pdfText =
//            TryExtractPdfText(pdfBytes);

//        pdfText =
//            CleanText(pdfText);

//        if (!string.IsNullOrWhiteSpace(pdfText)
//            && pdfText.Length > 100)
//        {
//            Console.WriteLine(
//                $"PdfPig extracted {pdfText.Length} chars");

//            return pdfText;
//        }

//        // STEP 2
//        // OCR fallback

//        Console.WriteLine(
//            "PdfPig found little text. Running OCR...");

//        var ocrText =
//            ExtractUsingOcr(pdfBytes);

//        ocrText =
//            CleanText(ocrText);

//        Console.WriteLine(
//            $"OCR extracted {ocrText.Length} chars");

//        return ocrText;
//    }

//    private string TryExtractPdfText(
//        byte[] pdfBytes)
//    {
//        try
//        {
//            using var pdf =
//                PdfDocument.Open(pdfBytes);

//            var text =
//                new StringBuilder();

//            foreach (var page in pdf.GetPages())
//            {
//                text.AppendLine(page.Text);
//            }

//            return text.ToString();
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine(
//                $"PdfPig Error: {ex.Message}");

//            return string.Empty;
//        }
//    }

//    private string ExtractUsingOcr(
//        byte[] pdfBytes)
//    {
//        var result =
//            new StringBuilder();

//        using var docReader =
//            DocLib.Instance.GetDocReader(
//                pdfBytes,
//                new PageDimensions(
//                    2480,
//                    3508));

//        int pageCount =
//            docReader.GetPageCount();

//        using var engine =
//            new TesseractEngine(
//                @"./tessdata",
//                "eng",
//                EngineMode.Default);

//        for (int i = 0; i < pageCount; i++)
//        {
//            using var pageReader =
//                docReader.GetPageReader(i);

//            var rawBytes =
//                pageReader.GetImage();

//            int width =
//                pageReader.GetPageWidth();

//            int height =
//                pageReader.GetPageHeight();

//            using var bitmap =
//                new Bitmap(
//                    width,
//                    height,
//                    PixelFormat.Format32bppArgb);

//            var bitmapData =
//                bitmap.LockBits(
//                    new Rectangle(
//                        0,
//                        0,
//                        width,
//                        height),
//                    ImageLockMode.WriteOnly,
//                    bitmap.PixelFormat);

//            System.Runtime.InteropServices.Marshal.Copy(
//                rawBytes,
//                0,
//                bitmapData.Scan0,
//                rawBytes.Length);

//            bitmap.UnlockBits(bitmapData);

//            using var pix =
//                PixConverter.ToPix(bitmap);

//            using var page =
//                engine.Process(pix);

//            result.AppendLine(
//                page.GetText());
//        }

//        return result.ToString();
//    }

//    private string CleanText(string text)
//    {
//        if (string.IsNullOrWhiteSpace(text))
//        {
//            return string.Empty;
//        }

//        text = text.Replace("\r", "");

//        // keep paragraph breaks
//        text = Regex.Replace(
//            text,
//            @"\n{3,}",
//            "\n\n");

//        // remove extra spaces but preserve new lines
//        text = Regex.Replace(
//            text,
//            @"[ ]{2,}",
//            " ");

//        return text.Trim();
//    }



//private List<DocumentChunk> CreateChunks(
//    string extractedText,
//    string s3Key)
//    {
//        const int maxChunkLength = 600;

//        var result =
//            new List<DocumentChunk>();

//        var lines =
//            extractedText.Split(
//                '\n',
//                StringSplitOptions.RemoveEmptyEntries);

//        var currentChunk =
//            new StringBuilder();

//        int order = 0;

//        foreach (var line in lines)
//        {
//            if (currentChunk.Length + line.Length > maxChunkLength)
//            {
//                result.Add(
//                    new DocumentChunk
//                    {
//                        Id = Guid.NewGuid(),
//                        DocumentId = s3Key,
//                        ParcelId = ExtractParcelId(s3Key),
//                        ChunkText = currentChunk.ToString(),
//                        ChunkType = "General",
//                        ChunkOrder = order++
//                    });

//                currentChunk.Clear();
//            }

//            currentChunk.AppendLine(line);
//        }

//        if (currentChunk.Length > 0)
//        {
//            result.Add(
//                new DocumentChunk
//                {
//                    Id = Guid.NewGuid(),
//                    DocumentId = s3Key,
//                    ParcelId = ExtractParcelId(s3Key),
//                    ChunkText = currentChunk.ToString(),
//                    ChunkType = "General",
//                    ChunkOrder = order++
//                });
//        }

//        return result;
//    }

    private async Task SaveChunksAsync(
     List<DocumentChunk> chunks)
    {
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