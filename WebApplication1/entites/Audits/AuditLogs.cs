namespace WebApplication1.entites.Audits
{
    public class AuditLogs
    {
        public int Id { get; set; }

        public string Action { get; set; }

        public string EntityType { get; set; }

        public string EntityId { get; set; }

        public string Description { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
