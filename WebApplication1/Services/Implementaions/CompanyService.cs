using WebApplication1.Dto.Company;
using WebApplication1.entites;
using WebApplication1.Repositories.Interfaces;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Services.Implementations
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _repository;
            public CompanyService(ICompanyRepository repository)

        {
            _repository=repository;
        }
        public Task<List<FetchAllCompanies>> GetAllAsync()
        {
            var response= _repository.GetAllAsync();
            return response;
            
        }
        public Task<FetchCompanyResponseDto> GetCompanyDetails(string companyName)
        {
            return _repository.GetCompanyDetails(companyName);
        }

        public async Task<Company> CreateAsync(CreateCompanyDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new Exception("Company name is required");
            }

            if (string.IsNullOrWhiteSpace(dto.GeoAccessType))
            {
                throw new Exception("GeoAccessType is required");
            }
            //var company = new Company
            //{
            //    Name = dto.Name,
            //    GeoAccessType = dto.GeoAccessType,
            //    CenterLat = dto.CenterLat,
            //    CenterLng = dto.CenterLng,
            //    RadiusKm = dto.RadiusKm,
            //    Polygon = dto.Polygon,
            //    CreatedDate = DateTime.UtcNow
            //};

            var result = await _repository.CreateAsync(dto);

            return result;
        }
    }
}