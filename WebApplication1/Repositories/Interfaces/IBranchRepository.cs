

using WebApplication1.Dto.Branch;
using WebApplication1.entites;
namespace WebApplication1.Repositories.Interfaces
{
    public interface IBranchRepository
    {
        Task<Branch> CreateBranchAsync(CreateBranch dto);

    }
}
