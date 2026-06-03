using WebApplication1.entites.StarterEntities;
using WebApplication1.GisEntities;

namespace WebApplication1.Dto.PropertySearch
{
    public class PropertyStaterDto
    {
        public Parcels Parcel { get; set; }
        public StarterPackage StarterPackage { get; set; }
        public List<StarterIssue> StarterIssues { get; set; }
    }
}
