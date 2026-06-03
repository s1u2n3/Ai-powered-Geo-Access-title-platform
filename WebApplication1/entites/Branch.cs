namespace WebApplication1.entites
{
    public class Branch
    {
        public int Id { get; set; }

        // Parent Company
        public int CompanyId { get; set; }

        public Company Company { get; set; }

        // Branch Info
        public string BranchCode { get; set; }

        public string BranchName { get; set; }

        public string BranchType { get; set; }

        // Operational Contact
        public string ManagerName { get; set; }

        public string ContactEmail { get; set; }

        public string ContactPhone { get; set; }

        // Address
        public string AddressLine1 { get; set; }

        public string City { get; set; }

        public string PostalCode { get; set; }

        // Operational Permissions
        public bool CanVerifyTitle { get; set; }

        public bool CanApproveInsurance { get; set; }

        public bool CanReviewDocuments { get; set; }

        // Flags
        public bool IsHeadOffice { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }
            = DateTime.UtcNow;

        // Navigation
        public ICollection<BranchStateCountyAccess> BranchStateCountyAccesses  { get; set; }

        public ICollection<ClientCompanyBranchAccess>
        ClientCompanyBranchAccesses
        { get; set; }
    }
}
