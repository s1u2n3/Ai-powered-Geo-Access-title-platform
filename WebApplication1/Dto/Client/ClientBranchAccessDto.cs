namespace WebApplication1.Dto.Client
{
    public class ClientBranchAccessDto
    {
        public string BranchCode { get; set; }

        public bool CanLoadStarter { get; set; }

        public bool CanReviewTitle { get; set; }

        public bool CanIssueInsurance
        { get; set; }
    }
}
