using Microsoft.EntityFrameworkCore;
using WebApplication1.Dto.PropertySearch;
using WebApplication1.entites.StarterEntities;
using WebApplication1.Repositories.Interfaces;

namespace WebApplication1.Repositories.implementations.PropertyStarer
{
    public class PropertyStarterRepository : IPropertyStarerRepository
    {
        private readonly PostgresDbContext _context;
        public PropertyStarterRepository (PostgresDbContext context)
        {
            _context = context;
        }

        public async Task<PropertyStaterDto> GetParcelWithStarterAsync(string parcelId)
        {
            var parcel = await _context.Parcels.FirstOrDefaultAsync(x => x.ParcelId == parcelId);

            if(parcel == null)
            {
                return null;
            }
            StarterPackage starterPackage = null;
            List<StarterIssue> stareterIssue = new();
            if (parcel.HasStarter)
            {
                starterPackage = await _context.StarterPackage.OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync(x => x.ParcelId == parcelId);

                if (starterPackage != null)
                {
                    stareterIssue = await _context.StarterIssue.Where(x => x.StarterPackageId == starterPackage.Id).ToListAsync();
                }
            }
            return new PropertyStaterDto
            {
                Parcel = parcel,
                StarterPackage = starterPackage,
                StarterIssues = stareterIssue
            };
        }
    }
    
}
