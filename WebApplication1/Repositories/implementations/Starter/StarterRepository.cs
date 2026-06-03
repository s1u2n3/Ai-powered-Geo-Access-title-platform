using Microsoft.EntityFrameworkCore;
using WebApplication1.entites.StarterEntities;
using WebApplication1.Repositories.Interfaces;

public class StarterRepository
    : IStarterRepository
{
    private readonly
        PostgresDbContext _context;

    public StarterRepository(
        PostgresDbContext context)
    {
        _context = context;
    }

    public async Task SaveStarterAsync(
    string parcelId,
    string currentOwner,
    int riskScore,
    string riskLevel,
    bool insuranceReady,
    List<TitleIssueDto> issues,
    string rawResponse)
    {
        var starterPackage =
            new StarterPackage
            {
                ParcelId = parcelId,

                CoverageFromYear = 2015,

                CoverageToYear = 2026,

                StarterType = "Full Starter",

                Status = "Completed",

                RiskScore = riskScore,
    

                RiskLevel =riskLevel,
  

                InsuranceReady =insuranceReady,


                CurrentOwner = currentOwner,

                StarterJson =
                    rawResponse,

                CreatedDate =
                    DateTime.UtcNow
            };

        _context.StarterPackage
            .Add(starterPackage);

        await _context
            .SaveChangesAsync();

        if (issues!= null)
        {
            foreach (var issue in issues)
            {
                _context.StarterIssue.Add(
                    new StarterIssue
                    {
                        StarterPackageId =
                            starterPackage.Id,

                        IssueType =
                            issue.IssueType,

                        Description =
                            issue.Description,

                        Severity =
                            issue.Severity,

                        Resolved =
                            false,

                        CreatedDate =
                            DateTime.UtcNow
                    });
            }

            await _context.SaveChangesAsync();
        }

        var documentIds =
            await _context.DocumentChunks
                .Where(x =>
                    x.ParcelId == parcelId)
                .Select(x =>
                    x.DocumentId)
                .Distinct()
                .ToListAsync();

        foreach (var documentId
            in documentIds)
        {
            _context.StarterDocument.Add(
                new StarterDocument
                {
                    StarterPackageId =
                        starterPackage.Id,

                    DocumentId =
                        documentId,

                    DocumentName =
                        documentId,

                    DocumentType =
                        "Source Document",

                    AddedDate =
                        DateTime.UtcNow
                });
        }

        await _context
            .SaveChangesAsync();
    }
}