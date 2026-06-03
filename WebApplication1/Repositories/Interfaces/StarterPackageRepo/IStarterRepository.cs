public interface IStarterRepository
{
    Task SaveStarterAsync(
     string parcelId,
     string currentOwner,
     int riskScore,
     string riskLevel,
     bool insuranceReady,
     List<TitleIssueDto> issues,
     string rawResponse);
}