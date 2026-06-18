using Newtonsoft.Json.Linq;

public class StarterAnalysisResponseDto
{
    public ParcelSummaryDto ParcelSummary { get; set; }

    public OwnershipDto Ownership { get; set; }

    public EncumbranceDto Encumbrances { get; set; }

    public LitigationDto Litigation { get; set; }

    public TaxInformationDto TaxInformation { get; set; }

    public ApprovalDto Approvals { get; set; }

    public List<TitleIssueDto> TitleIssues { get; set; }

    public RiskAssessmentDto RiskAssessment { get; set; }

    public List<MissingDocumentDto> MissingDocuments { get; set; }

    public List<string> ReviewRecommendations { get; set; }

    public List<string> AiObservations { get; set; }

    public JObject StarterFacts { get; set; }
}

public class ParcelSummaryDto
{
    public string ParcelId { get; set; }

    public string SurveyNumber { get; set; }

    public string Village { get; set; }

    public string Taluk { get; set; }

    public string District { get; set; }

    public string Extent { get; set; }

    public string LandType { get; set; }
}

public class OwnershipDto
{
    public string CurrentOwner { get; set; }

    public string OwnershipSummary { get; set; }

    public string OwnershipStatus { get; set; }
}

public class EncumbranceDto
{
    public string Status { get; set; }

    public List<string> Findings { get; set; }
}

public class LitigationDto
{
    public string Status { get; set; }

    public List<string> Findings { get; set; }
}

public class TaxInformationDto
{
    public string TaxStatus { get; set; }

    public string LatestTaxYear { get; set; }
}

public class ApprovalDto
{
    public string BuildingApproval { get; set; }

    public string ConversionOrder { get; set; }

    public string OccupancyCertificate { get; set; }
}

public class TitleIssueDto
{
    public string IssueType { get; set; }

    public string Severity { get; set; }

    public string Description { get; set; }

    public string RecommendedAction { get; set; }
}

public class RiskAssessmentDto
{
    public string OverallRisk { get; set; }

    public List<RiskFactorDto> RiskFactors { get; set; }
}

public class RiskFactorDto
{
    public string Factor { get; set; }

    public string Severity { get; set; }
}

public class MissingDocumentDto
{
    public string DocumentType { get; set; }

    public string Reason { get; set; }
}