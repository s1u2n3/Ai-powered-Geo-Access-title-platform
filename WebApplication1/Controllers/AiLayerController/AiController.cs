using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Dto;
using WebApplication1.Services.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class AIController
    : ControllerBase
{
    private readonly
        IAIAnalysisService
            _aiService;

    public AIController(
        IAIAnalysisService aiService)
    {
        _aiService = aiService;
    }
    [Authorize]
    [HttpPost("analyze")]
    public async Task<IActionResult>
        AnalyzeParcel(
            [FromBody]
            ParcelQuestionRequest request)
    {
        var result =
            await _aiService
                .AnalyzeParcelAsync(
                    request.ParcelId,
                    request.Question);

        return Ok(result);
    }
}