
using WebApplication1.entites;
using WebApplication1.Dto;
using WebApplication1.Services;
using WebApplication1.Repositories.Interfaces;
using WebApplication1.Dto.Branch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
namespace WebApplication1.Repositories.implementations
{
    public class BranchRepository:IBranchRepository
    {
        private readonly AppDbContext _context;

        public BranchRepository (AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<BranchResponse>>GetCompanyBasedBranch(string companyName){

           return await _context.Branches
     .Include(b => b.Company)
     .Where(b => b.Company.Name== companyName)
         .Select(x => new BranchResponse
        {
            Id = x.Id,
            BranchCode=x.BranchCode
           
        })
        .ToListAsync();
        }
        public async Task<Branch> CreateBranchAsync(CreateBranch dto) {

            var company = await _context.Companies.Include(x => x.CompanyStateCountyAccesses).
                    FirstOrDefaultAsync(x => x.Name == dto.CompanyName);
            if(company == null)
            {
                throw new Exception("Invalid Company");
            }
            var branch = new Branch
            {
                CompanyId = company.Id,

                BranchCode = dto.BranchCode,

                BranchName = dto.BranchName,

                BranchType = dto.BranchType,

                ManagerName = dto.ManagerName,

                ContactEmail = dto.ContactEmail,

                ContactPhone = dto.ContactPhone,

                AddressLine1 = dto.AddressLine1,

                City = dto.City,

                PostalCode = dto.PostalCode,

                CanVerifyTitle = dto.CanVerifyTitle,

                CanApproveInsurance =
            dto.CanApproveInsurance,

                CanReviewDocuments =
            dto.CanReviewDocuments,

                IsHeadOffice = dto.IsHeadOffice
            };
            await _context.Branches.AddAsync(branch);
            await _context.SaveChangesAsync();

            //addloactions
            foreach (var location in dto.Locations)
            {
                var county = await _context.Counties.Include(x => x.State)
                    .FirstOrDefaultAsync(x => x.CountyCode == location.CountyCode &&

                    x.State.StateCode == location.StateCode);
                if(county == null)
                {
                    throw new Exception("Invalid county Entered ");
                }

                // verify comany geo access///

                var companyAccess = await _context.CompanyStateCountyAccesses.AnyAsync(x => x.CompanyId == company.Id && x.CountyId == county.Id);

                if(companyAccess == null)
                {
                    throw new Exception("county is not access by parent company");
                }

                var branchCountyAcces = new BranchStateCountyAccess
                {
                    BranchId = branch.Id,
                    CountyId = county.Id
                };
                await _context.BranchStateCountyAccesses.AddAsync(branchCountyAcces);
            }
            await _context.SaveChangesAsync();
            return branch;
        }
    }
}
