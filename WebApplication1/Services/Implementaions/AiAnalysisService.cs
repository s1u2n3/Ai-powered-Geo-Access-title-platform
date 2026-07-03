using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.VisualBasic;
using NetTopologySuite.Operation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using UglyToad.PdfPig;
using WebApplication1.Dto.LLM;
using WebApplication1.entites;
using WebApplication1.Repositories.implementations;
using WebApplication1.Services.Interfaces;
using WebApplication1.Services.Interfaces.AiServices;
using static System.Runtime.InteropServices.JavaScript.JSType;


public class AIAnalysisService
    : IAIAnalysisService
{
    private readonly
        PostgresDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly IStarterRepository _starterRepository;
    private readonly IVectorSearchInterface _vectorSearchService;
    private readonly IConfiguration _configuration;



    public AIAnalysisService(PostgresDbContext context ,IConfiguration configuration,HttpClient httpClient, IStarterRepository  starterRepository,IVectorSearchInterface vectorSearchInterface)
    {
        _context = context;
        _httpClient=httpClient;
        _starterRepository = starterRepository;
        _vectorSearchService = vectorSearchInterface;
        _configuration = configuration;
    }

    public async Task<string>
        AnalyzeParcelAsync(
            string parcelId,
            string question)
     {
        //var chunks =
        //    await _context.DocumentChunks
        //        .Where(x =>
        //            x.ParcelId == parcelId)
        //        .OrderBy(x =>
        //            x.ChunkOrder)

        //        .Select(x =>
        //            x.ChunkText)
        //        .ToListAsync();

        //var combinedText =
        //    string.Join(
        //        "\n\n",
        //        chunks);

        //combinedText =
        //    SanitizeText(combinedText);
        var ownershipChunks =
    await _vectorSearchService
        .SearchAsync(
            parcelId,
        
            "Identify current owner, previous owners, ownership history, title holders, transfer deeds, sale deeds, inheritance records, gift deeds, mutation entries and ownership changes..");


        foreach (var chunk in ownershipChunks)
        {
            Console.WriteLine(chunk.Distance);
        }
        var litigationChunks =
            await _vectorSearchService
                .SearchAsync(
                    parcelId,
                    "Identify litigation, court cases, legal disputes, notices and pending proceedings.");

        foreach (var chunk in litigationChunks)
        {
            Console.WriteLine(chunk.Distance);
        }

        var encumbranceChunks =
            await _vectorSearchService
                .SearchAsync(
                    parcelId,
                    "Identify mortgages, liens, charges, loans and encumbrances.");

        foreach (var chunk in encumbranceChunks)
        {
            Console.WriteLine(chunk.Distance);
        }


        var ownershipContext = string.Join(
     "\n\n",
     ownershipChunks
         .DistinctBy(x => x.Chunk.ChunkText)
         .Select(x => x.Chunk.ChunkText)
 );

        var litigationContext =
            string.Join(
                "\n\n",
                litigationChunks.DistinctBy(x=>x.Chunk.ChunkText).Select(x =>
                    x.Chunk.ChunkText));

        var encumbranceContext =
            string.Join(
                "\n\n",
                encumbranceChunks.DistinctBy(x=>x.Chunk.ChunkText).Select(x => x.Chunk.ChunkText));
        var prompt = $@"
You are a senior title insurance examiner.

Your task is to generate a Starter Package JSON from the provided evidence.

IMPORTANT RULES:

1. Return VALID JSON ONLY.
2. NEVER return markdown.
3. NEVER return explanations.
4. NEVER omit any property from the schema.
5. If evidence is missing, keep the property and use:

   * """" for strings
   * false for booleans
   * [] for arrays
   * {{}} for objects

6. Use ONLY information present in the evidence.
7. Do NOT invent facts.
8. Do NOT infer ownership, litigation, encumbrances, taxes, approvals, or risks without evidence.
9. Every property in the schema MUST exist even when empty.
10. titleIssues must contain structured objects.
11. riskAssessment.overallRisk must always be one of:
    LOW, MEDIUM, HIGH, CRITICAL.

RISK RULES:

* Active litigation or pending court case = HIGH
* Boundary dispute = HIGH
* Active mortgage = MEDIUM
* Released mortgage with no issues = LOW
* No significant issues found = LOW
* Multiple severe title issues = CRITICAL

OWNERSHIP EVIDENCE:

{ownershipContext}

LITIGATION EVIDENCE:

{litigationContext}

ENCUMBRANCE EVIDENCE:

{encumbranceContext}

RETURN THIS EXACT JSON STRUCTURE:

{{
  ""parcelSummary"": {{
    ""propertyExtent"": """"
  }},

  ""ownership"": {{
    ""currentOwner"": """",
    ""previousOwners"": [],
    ""ownershipHistory"": [],
    ""recordsUpdatedThrough"": """"
  }},

  ""encumbrances"": {{
    ""active"": false,
    ""mortgage"": """",
    ""status"": """",
    ""details"": []
  }},

  ""litigation"": [
    {{
      ""caseNumber"": """",
      ""court"": """",
      ""caseType"": """",
      ""status"": """",
      ""nextHearing"": """",
      ""description"": """"
    }}
  ],

  ""taxInformation"": {{
    ""paid"": false,
    ""fy"": """",
    ""status"": """"
  }},

  ""approvals"": [],

  ""titleIssues"": [
    {{
      ""issueType"": """",
      ""severity"": """",
      ""description"": """",
      ""recommendedAction"": """"
    }}
  ],

  ""riskAssessment"": {{
    ""overallRisk"": ""LOW"",
    ""riskFactors"": []
  }},

  ""missingDocuments"": [
    {{
      ""type"": """",
      ""description"": """"
    }}
  ],

  ""reviewRecommendations"": [
    {{
      ""type"": """",
      ""description"": """"
    }}
  ],

  ""aiObservations"": [
    {{
      ""observation"": """"
    }}
  ],

  ""starterFacts"": [
    {{
      ""category"": """",
      ""name"": """",
      ""value"": """"
    }}
  ]
}}
";




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
     root["ownership"]?["currentOwner"]?
         .ToString() ?? "";
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


    //private async Task<string>CallLLMAsync(string prompt)
    //{
    //    var request = new
    //    {
    //        model = "llama3:8b",
    //        prompt = prompt,
    //        stream = false,
    //        format = "json",
    //        options = new
    //        {
    //            temperature = 0.1
    //        }

    //    };

    //    var response =
    //        await _httpClient
    //            .PostAsJsonAsync(
    //                "http://localhost:11434/api/generate",
    //                request);
    //    var result =
    //await response.Content
    //    .ReadFromJsonAsync<LlmResponse>();



    //    return result.response;



    //}

    private async Task<string> CallLLMAsync(string prompt)
    {
        var apiKey = _configuration["Gemini:ApiKey"];

        var request = new
        {
            contents = new[]
            {
            new
            {
                parts = new[]
                {
                    new
                    {
                        text = prompt
                    }
                }
            }
        },
            generationConfig = new
            {
                temperature = 0.1,
                responseMimeType = "application/json"
            }
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key={apiKey}",
            request); var responseText =
    await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content.ReadFromJsonAsync<GeminiResponse>();

        return result.Candidates[0]
                     .Content
                     .Parts[0]
                     .Text;
    }
}
