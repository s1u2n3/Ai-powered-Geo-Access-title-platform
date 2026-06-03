namespace WebApplication1.entites.StarterEntities
{
    public class StarterIssue
    {
        public int Id { get; set; }

        public int StarterPackageId { get; set; }

        public string ?  IssueType { get; set; }
        // Missing Document
        // Open Mortgage
        // Tax Issue
        // Court Case
        // Boundary Issue
        // Judgment

        public string ? Description { get; set; }

        public string ? Severity { get; set; }
        // Low
        // Medium
        // High
        // Critical

        public bool ? Resolved { get; set; }

        public DateTime CreatedDate { get; set; }

        public StarterPackage StarterPackage { get; set; }
    }
}
