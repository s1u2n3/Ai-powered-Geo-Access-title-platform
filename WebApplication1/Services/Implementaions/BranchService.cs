using WebApplication1.Dto;
using WebApplication1.Dto.Branch;
using WebApplication1.entites;
using WebApplication1.Repositories.implementations;
using WebApplication1.Repositories.Interfaces;
using WebApplication1.Services;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Services.Implementaions
{
    public class BranchService : IBranchService
    {
        private readonly IBranchRepository
            _repository;

        public BranchService(
            IBranchRepository repository)
        {
            _repository = repository;
        }

        public async Task<Branch> CreateBranchAsync(CreateBranch dto)
        {
            return await _repository.CreateBranchAsync(dto);
        }
        public async Task<List<BranchResponse>> GetComanyBasedBranch(string comanyName)
        {
            return await _repository.GetCompanyBasedBranch(comanyName);
        }
    }
}
