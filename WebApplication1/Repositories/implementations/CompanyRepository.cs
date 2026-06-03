
using WebApplication1.Repositories.Interfaces;
using WebApplication1.Dto;
using WebApplication1.entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Routing;
using WebApplication1.Dto.Company;

namespace WebApplication1.Repositories.implementations
{
    public class CompanyRepository:ICompanyRepository
    {
        private readonly AppDbContext _context;
        public CompanyRepository(AppDbContext context)
        {
            _context = context;
        }

        //get All companies
        public async Task<List<FetchAllCompanies>> GetAllAsync()
        {
            return await _context.Companies.Select(x => new FetchAllCompanies
            {
                Id = x.Id,
                Name = x.Name
            }).ToListAsync();
        }
        public async Task<Company>CreateAsync(CreateCompanyDto dto)
        {
            var company = new Company
            {
                Name = dto.Name,
                GeoAccessType = dto.GeoAccessType,
                CenterLat = dto.CenterLat,
                CenterLng = dto.CenterLng,
                RadiusKm = dto.RadiusKm,
                Polygon = dto.Polygon,
                CreatedDate = DateTime.UtcNow
            };

            await _context.Companies.AddAsync(company);

            await _context.SaveChangesAsync();

            // SAVE GEO ACCESS
            foreach (var location in dto.Locations)
            {
                var county =
                    await _context.Counties
                        .Include(x => x.State)
                        .FirstOrDefaultAsync(x =>
                            x.CountyCode ==
                            location.CountyCode
                            &&
                            x.State.StateCode ==
                            location.StateCode);

                if (county == null)
                    continue;

                var access =
                    new CompanyStateCountyAccess
                    {
                        CompanyId = company.Id,
                        CountyId = county.Id
                    };

                await _context
                    .CompanyStateCountyAccesses
                    .AddAsync(access);
            }

            await _context.SaveChangesAsync();

            return company;
        }
        public async Task<FetchCompanyResponseDto> GetCompanyDetails (string companyName)
        {
            var company = await _context.Companies.Include(x => x.CompanyStateCountyAccesses).ThenInclude(x => x.County).ThenInclude(x => x.State).
                 FirstOrDefaultAsync(x => x.Name == companyName);
            if(company== null)
            {
                return null;
            }

            var locations = company.CompanyStateCountyAccesses.GroupBy(x => x.County.State).Select(stateGroup =>
            new StateAndCountyResponse
            {
                StateCode = stateGroup.Key.StateCode,
                StateName = stateGroup.Key.StateName,
                StateLatitude = stateGroup.Key.Latitude,
                StateLongitude = stateGroup.Key.Longitude,
                Counties = stateGroup.Select(x => new CountyItemDto
                {
                    CountyCode = x.County.CountyCode,
                    CountyName = x.County.CountyName,
                    CountyLatitude = x.County.Latitude,
                    CountyLongitude = x.County.Longitude
                }).ToList()


            }).ToList();
            return new FetchCompanyResponseDto
            {
                Name = company.Name,
                GeoAccessType = company.GeoAccessType,
                CenterLat = company.CenterLat,
                CenterLng = company.CenterLng,
                RadiusKm = company.RadiusKm,
                Locations = locations

            };
        }
    }
   

}
