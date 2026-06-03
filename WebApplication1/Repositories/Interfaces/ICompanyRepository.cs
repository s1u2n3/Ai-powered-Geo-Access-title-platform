using WebApplication1.Dto.Company;
using WebApplication1.entites;

namespace WebApplication1.Repositories.Interfaces
{
    public interface ICompanyRepository
    
         {
        Task<List<FetchAllCompanies>> GetAllAsync();

        Task<Company> CreateAsync(CreateCompanyDto data);
        Task<FetchCompanyResponseDto> GetCompanyDetails(string CompanyName);
}
}
