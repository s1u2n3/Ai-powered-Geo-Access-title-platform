namespace WebApplication1.Dto.Client;

public class ClientResponseDto
{
    // CLIENT DETAILS

    public int ClientId { get; set; }

    public string ClientCode { get; set; }

    public string ClientName { get; set; }

    public string ClientType { get; set; }

    public string Email { get; set; }

    public string PhoneNumber { get; set; }

    // MULTIPLE COMPANIES

    public List<ClientCompanyResponseDto>
        Companies
    { get; set; }
}

public class ClientCompanyResponseDto
{
    // COMPANY DETAILS

    public int CompanyId { get; set; }

    public string CompanyName { get; set; }

    // SAME COMPANY
    // CAN HAVE MULTIPLE BRANCHES

    public List<ClientBranchResponseDto>
        Branches
    { get; set; }
}

public class ClientBranchResponseDto
{
    // BRANCH DETAILS

    public int BranchId { get; set; }

    public string BranchCode { get; set; }

    public string BranchName { get; set; }

    public string BranchType { get; set; }

    // CLIENT PERMISSIONS
    // FOR THIS BRANCH

    public bool CanLoadStarter { get; set; }

    public bool CanReviewTitle { get; set; }

    public bool CanIssueInsurance { get; set; }

    // GEO ACCESS
    // FOR THIS BRANCH

    public List<StateAndCountyResponse>
        GeoAccess
    { get; set; }
}