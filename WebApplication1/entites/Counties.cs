namespace WebApplication1.entites
{
    public class County
    {
        public int Id { get; set; }

        public string CountyCode { get; set; }

        public string CountyName { get; set; }

        // FK
        public int StateId { get; set; }

        // Navigation
        public States State { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public bool IsDisabled { get; set; } = false;

        public DateTime CreatedDate { get; set; }
            = DateTime.UtcNow;
    }

}
