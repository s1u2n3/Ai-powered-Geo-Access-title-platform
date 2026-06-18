using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services.Implementaions;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Controllers.PostGisController
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly S3Service _s3service;

        private readonly
            IDocumentProcessingService
                _processingService;

        public DocumentController(
            S3Service s3service,
            IDocumentProcessingService
                processingService)
        {
            _s3service = s3service;

            _processingService =
                processingService;
        }
        [Authorize]
        [HttpGet("ChunkDocuments{parcelId}")]
        public async Task<IActionResult>
            GetParcelDocumentsForchunkings(
                string parcelId)
        {

            var JobId =  await _processingService.CreateDocumentProcessingJobAsync(parcelId);

            return Ok(new
            {
                JobId,
                Message = "Document processing started"

            });
            //var documents =
            //    await _s3service
            //        .GetParcelDocumentsAsync(
            //            parcelId);

            //await _processingService
            //    .ProcessParcelDocumentsAsync(
            //        documents);

            // var result =
            //    documents.Select(x => new
            //    {
            //        DocumentName =
            //            Path.GetFileName(x.Key),

            //        S3Key = x.Key
            //    });

            //return Ok(result);
        }
    }
}