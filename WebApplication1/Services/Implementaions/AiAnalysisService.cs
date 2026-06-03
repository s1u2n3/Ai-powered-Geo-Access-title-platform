using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using WebApplication1.Dto.LLM;
using WebApplication1.Repositories.implementations;
using WebApplication1.Services.Interfaces;


public class AIAnalysisService
    : IAIAnalysisService
{
    private readonly
        PostgresDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly IStarterRepository _starterRepository;
        

    public AIAnalysisService(PostgresDbContext context ,HttpClient httpClient, IStarterRepository  starterRepository)
    {
        _context = context;
        _httpClient=httpClient;
        _starterRepository = starterRepository;
    }

    public async Task<string>
        AnalyzeParcelAsync(
            string parcelId,
            string question)
     {
        var chunks =
            await _context.DocumentChunks
                .Where(x =>
                    x.ParcelId == parcelId)
                .OrderBy(x =>
                    x.ChunkOrder)
                
                .Select(x =>
                    x.ChunkText)
                .ToListAsync();

        var combinedText =
            string.Join(
                "\n\n",
                chunks);

        combinedText =
            SanitizeText(combinedText);
        //        var prompt = $@"
        //You are a JSON extraction engine.
        //input
        //{question}


        //Your only task is to extract title information from property documents.

        //IMPORTANT:

        //- Return ONLY valid JSON.
        //- Do NOT explain.
        //- Do NOT summarize.
        //- Do NOT provide recommendations.
        //- Do NOT provide notes.
        //- Do NOT provide analysis.
        //- Do NOT return markdown.
        //- Do NOT return text before JSON.
        //- Do NOT return text after JSON.
        //- Use ONLY information present in the documents.
        //- Do NOT invent facts.
        //- Do NOT guess values.
        //- If information is unavailable use:
        //  - Empty string """"
        //  - Empty array []

        //Return EXACTLY this JSON structure:

        //{{
        //  ""vestingInformation"": {{
        //    ""currentOwners"": []
        //  }},

        //  ""taxInformation"": {{
        //    ""taxStatus"": """"
        //  }},

        //  ""titleIssues"": [
        //    {{
        //      ""issueType"": """",
        //      ""severity"": """",
        //      ""description"": """",
        //      ""recommendedAction"": """"
        //    }}
        //  ],

        //  ""riskAssessment"": {{
        //    ""overallRisk"": """",
        //    ""riskFactors"": [
        //      {{
        //        ""factor"": """",
        //        ""severity"": """"
        //      }}
        //    ]
        //  }}
        //}}

        //Extraction Rules:

        //Current Owners:
        //- Extract from Sale Deeds
        //- Extract from Mutation Records
        //- Extract from Khata Extracts
        //- Extract from Ownership Records

        //Tax Status:
        //- Extract from Property Tax Receipts
        //- Extract from Tax Records

        //Title Issues:
        //Create titleIssues only when supported by evidence.

        //Possible issues:
        //- Litigation
        //- Court Case
        //- Boundary Dispute
        //- Active Mortgage
        //- Encumbrance
        //- Judgment
        //- Ownership Concern
        //- Missing Document
        //- Legal Heir Issue

        //Risk Assessment:

        //LOW:
        //- No significant issues found

        //MEDIUM:
        //- Active Mortgage
        //- Minor Boundary Concern
        //- Minor Encumbrance

        //HIGH:
        //- Litigation
        //- Court Case
        //- Judgment
        //- Multiple Significant Issues

        //CRITICAL:
        //- Severe ownership conflict
        //- Multiple unresolved major issues

        //Validation Rules:

        //- The response MUST start with {{
        //- The response MUST end with }}
        //- Every property must exist.
        //- Never rename properties.
        //- Never add properties.
        //- Never remove properties.

        //Parcel Documents:

        //{combinedText}
        //"

        var prompt = $@"
You are a senior Title Insurance Examiner.
Return exactly one valid JSON object. The response must be parseable by System.Text.Json and contain no text before or after the JSON.

Analyze all property documents.

Your goal is to produce:

1. A mandatory Starter Summary.
2. Additional document facts.

Rules:

* Use ONLY document evidence.
* Do NOT invent facts.
* Do NOT guess values.
* Do NOT summarize.
* Do NOT explain.
* Do NOT provide notes.
* Return ONLY valid JSON.
*All year ranges must be strings.



MANDATORY SECTIONS

Always populate:

* vestingInformation
* taxInformation
* titleIssues
* riskAssessment

Even if values are empty.

If information is unavailable:

* Use empty string """"
* Use empty array []

TITLE ISSUE RULES

Create titleIssues only when supported by evidence.

Examples:

* Litigation
* Court Case
* Judgment
* Active Mortgage
* Boundary Dispute
* Encumbrance
* Ownership Concern
* Missing Required Document
* Legal Heir Concern

RISK RULES

LOW

* Clear ownership
* Taxes paid
* No active litigation
* No active encumbrance

MEDIUM

* Active mortgage
* Minor title concern
* Minor encumbrance

HIGH

* Litigation
* Court case
* Judgment
* Ownership concern

CRITICAL

* Ownership conflict
* Multiple severe issues

ADDITIONAL FACTS

All other information found in documents must be stored inside:

starterFacts

Examples:

* Sale Deed
* Mortgage Deed
* Mortgage Release
* Khata Extract
* Mutation Register
* Survey Sketch
* Building Approval
* Occupancy Certificate
* Land Conversion Order
* Encumbrance Certificate
* Court Records
* Legal Heir Affidavit
* Owner KYC
* Tax Receipts
* Any other property-related document

Return EXACTLY this JSON structure:

{{
""vestingInformation"": {{
""currentOwners"": []
}},

""taxInformation"": {{
""taxStatus"": """"
}},

""titleIssues"": [],

""riskAssessment"": {{
""overallRisk"": """",
""riskFactors"": []
}},

""starterFacts"": {{}}
}}

Inside starterFacts:

* Create keys based on document meaning.
* Preserve important values.
* Use nested objects where appropriate.
* Include only facts supported by evidence.

Example:

{{
""vestingInformation"": {{
""currentOwners"": [""Priya Sharma""]
}},

""taxInformation"": {{
""taxStatus"": ""Paid FY 2025-26""
}},

""titleIssues"": [],

""riskAssessment"": {{
""overallRisk"": ""LOW"",
""riskFactors"": []
}},

""starterFacts"": {{
""saleDeed"": {{
""seller"": ""R Kumar"",
""buyer"": ""Priya Sharma"",
""value"": ""9250000""
}},
""mortgageRelease"": {{
""status"": ""Closed""
}},
""khataExtract"": {{
""khataNumber"": ""KH-2218""
}}
}}
}}

CRITICAL:

The response MUST start with {{

The response MUST end with }}

Do not return explanations.
Do not return notes.
Do not return markdown.

Return JSON only.

Documents:

{combinedText}
";




        Console.WriteLine($"Combined Length: {combinedText.Length}");

        var llmResponse =
            await CallLLMAsync(prompt);

        var cleanJson =
     ExtractJson(llmResponse);

//        try
//        {
//            JObject.Parse(cleanJson);
//        }
//        catch
//        {
//            var repairPrompt = $@"
//You are a JSON repair engine.

//Fix ONLY the JSON syntax.

//Rules:
//- Do not add data.
//- Do not remove data.
//- Do not change meanings.
//- Add missing braces if required.
//- Add missing quotes if required.
//- Fix invalid commas.
//- Ensure all property names use double quotes.
//- Ensure all string values use double quotes.
//- Return ONLY valid JSON.

//JSON:

//{cleanJson}
//";

//            var repairedResponse =
//                await CallLLMAsync(repairPrompt);

//            cleanJson =
//                ExtractJson(repairedResponse);
//        }

        var root = JObject.Parse(cleanJson);

       
        var currentOwner =
    root["vestingInformation"]?["currentOwners"]?
        .FirstOrDefault()?.ToString() ?? "";
        var riskLevel =
    root["riskAssessment"]?["overallRisk"]?
        .ToString() ?? "Low";
        //    var issues =
        //root["titleIssues"]?
        //    .ToObject<List<TitleIssueDto>>()
        //?? new List<TitleIssueDto>();
       

        var issues = new List<TitleIssueDto>();

        var titleIssuesToken = root["titleIssues"];

        if (titleIssuesToken is JArray issueArray)
        {
            foreach (var issue in issueArray)
            {
                if (issue.Type == JTokenType.String)
                {
                    issues.Add(new TitleIssueDto
                    {
                        IssueType = issue.ToString(),
                        Severity = "Unknown",
                        Description = "",
                        RecommendedAction = ""
                    });
                }
                else
                {
                    issues.Add(issue.ToObject<TitleIssueDto>());
                }
            }
        }
        var riskScore = riskLevel.ToUpper() switch
        {
            "LOW" => 20,
            "MEDIUM" => 50,
            "HIGH" => 80,
            "CRITICAL" => 100,
            _ => 20
        };
        var insuranceReady =
    riskScore < 50;

        //  var starterSummary =
        //JsonConvert.DeserializeObject<StarterAnalysisResponseDto>(
        //    cleanJson);
        //  var riskScore = CalculateRiskScore(starterSummary);
        //  var riskLevel = GetRiskLevel(riskScore);
        //  var insuranceReady = CalculateInsuranceReady(starterSummary, riskScore);


        //  var currentOwner =
        //      starterSummary?
        //          .VestingInformation?
        //          .CurrentOwners?
        //          .FirstOrDefault() ?? "";

        //  var issues =
        //      starterSummary?.TitleIssues
        //      ?? new List<TitleIssueDto>();
        await _starterRepository.SaveStarterAsync(
             parcelId,
             currentOwner,
             riskScore,
             riskLevel,
             insuranceReady,
             issues,
             cleanJson);

        return llmResponse;
    }
   

    private string ExtractJson(string response)
    {
        var start = response.IndexOf('{');
        var end = response.LastIndexOf('}');

        if (start == -1 || end == -1)
        {
            throw new Exception("Valid JSON not found.");
        }

        return response.Substring(
            start,
            end - start + 1);
    }

    private int CalculateRiskScore(
    StarterAnalysisResponseDto starter)
    {
        var score = 0;

        if (starter?.TitleIssues != null)
        {
            foreach (var issue in starter.TitleIssues)
            {
                switch (issue?.Severity?.ToUpper())
                {
                    case "CRITICAL":
                        score += 40;
                        break;

                    case "HIGH":
                        score += 25;
                        break;

                    case "MEDIUM":
                        score += 15;
                        break;

                    case "LOW":
                        score += 5;
                        break;
                }
            }
        }

        return Math.Min(score, 100);
    }
    private string GetRiskLevel(
    int riskScore)
    {
        if (riskScore <= 20)
            return "Low";

        if (riskScore <= 50)
            return "Medium";

        if (riskScore <= 80)
            return "High";

        return "Critical";
    }
    private bool CalculateInsuranceReady(
      StarterAnalysisResponseDto starter,
      int riskScore)
    {
        return riskScore < 50;
    }

    private string SanitizeText(
        string text)
    {
        return text
            .Replace("Ramesh", "[REDACTED]");
    }


    private async Task<string>
   CallLLMAsync(string prompt)
    {
        var request = new
        {
            model = "llama3:8b",
            prompt = prompt,
            stream = false,
            format = "json",
            options = new
            {
                temperature = 0.1
            }

        };

        var response =
            await _httpClient
                .PostAsJsonAsync(
                    "http://localhost:11434/api/generate",
                    request);
        var result =
    await response.Content
        .ReadFromJsonAsync<LlmResponse>();

        

        return result.response;

   
    }
}
