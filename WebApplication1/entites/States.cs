using System.Diagnostics.Metrics;

namespace WebApplication1.entites
{
    public class States
    {
        public int Id { get; set; }

        public string StateCode { get; set; }

        public string StateName { get; set; }

        public string CountryCode { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public bool IsDisabled { get; set; } = false;

        public DateTime CreatedDate { get; set; }
            = DateTime.UtcNow;

        public ICollection<County > Counties { get; set; }
    }
}
