namespace WebApplication1.entites.StarterEntities
{
    public class StarterDocument
    {
        public int Id { get; set; }

        public int StarterPackageId { get; set; }

        public string ? DocumentId { get; set; }

        public string ? DocumentName { get; set; }

        public string ? DocumentType { get; set; }

        public DateTime AddedDate { get; set; }
    }
}
