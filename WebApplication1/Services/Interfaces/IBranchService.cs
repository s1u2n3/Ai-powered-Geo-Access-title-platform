using WebApplication1.entites;
using WebApplication1.Dto;
using WebApplication1.Dto.Branch;
namespace WebApplication1.Services.Interfaces
    
{
    public interface IBranchService
    {
        Task<Branch> CreateBranchAsync(CreateBranch dto);
    }
}
