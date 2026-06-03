

namespace WebApplication1.entites
{
    public class ClientCompanyBranchAccess
    {
        public int Id { get; set; }

        // Client
        public int ClientId { get; set; }

        public Client Client { get; set; }

        // Company
        public int CompanyId { get; set; }

        public Company Company { get; set; }

        // Branch
        public int BranchId { get; set; }

        public Branch Branch { get; set; }

        // Permissions
        public bool CanLoadStarter { get; set; }

        public bool CanReviewTitle { get; set; }

        public bool CanIssueInsurance
        { get; set; }

        public bool IsDisabled { get; set; }
            = false;

        // Audit
        public DateTime CreatedDate
        { get; set; }
            = DateTime.UtcNow;
    }
}