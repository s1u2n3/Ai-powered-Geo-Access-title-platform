namespace WebApplication1.Services.Interfaces
{
    public interface IAIAnalysisService
    {
        Task<string> AnalyzeParcelAsync(
            string parcelId,
            string question);
    }
}
