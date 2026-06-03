using Newtonsoft.Json.Linq;

public class StarterAnalysisResponseDto
{
    public VestingInformationDto VestingInformation { get; set; }

    public TaxInformationDto TaxInformation { get; set; }

    public List<TitleIssueDto> TitleIssues { get; set; }

    public RiskAssessmentDto RiskAssessment { get; set; }

    public JObject StarterFacts { get; set; }
}
public class VestingInformationDto
{
    public List<string> CurrentOwners { get; set; }
}
public class TaxInformationDto
{
    public string TaxStatus { get; set; }
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