using WebApplication1.Dto.Company;
using WebApplication1.entites;

namespace WebApplication1.Services.Interfaces
{
    public interface ICompanyService
    {
        Task<List<FetchAllCompanies>> GetAllAsync();
        Task<Company> CreateAsync(CreateCompanyDto dto);
        Task<FetchCompanyResponseDto> GetCompanyDetails(string companyName);

    }
}
