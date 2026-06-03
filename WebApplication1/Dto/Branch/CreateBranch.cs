namespace WebApplication1.Dto.Branch
{
    public class CreateBranch
    {
           public string CompanyName { get; set; }

    public string BranchCode { get; set; }

    public string BranchName { get; set; }

    public string BranchType { get; set; }

    public string ManagerName { get; set; }

    public string ContactEmail { get; set; }

    public string ContactPhone { get; set; }

    public string AddressLine1 { get; set; }

    public string City { get; set; }

    public string PostalCode { get; set; }

    public bool CanVerifyTitle { get; set; }

    public bool CanApproveInsurance { get; set; }

    public bool CanReviewDocuments { get; set; }

    public bool IsHeadOffice { get; set; }

    public List<BranchGeoAccessDto>Locations{ get; set; }
      }
    }

