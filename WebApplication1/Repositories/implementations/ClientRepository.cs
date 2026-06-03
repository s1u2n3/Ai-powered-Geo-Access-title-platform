using Microsoft.EntityFrameworkCore;
using WebApplication1.Dto;
using WebApplication1.Dto.Client;
using WebApplication1.entites;
using WebApplication1.Repositories.Interfaces;

namespace WebApplication1.Repositories.Implementations
{
    public class ClientRepository
        : IClientRepository
    {
        private readonly AppDbContext _context;

        public ClientRepository(
            AppDbContext context)
        {
            _context = context;
        }

        public async Task<Client>
            CreateAsync(CreateClientDto dto)
        {
            var client = new Client
            {
                ClientCode = dto.ClientCode,
                ClientName = dto.ClientName,
                ClientType = dto.ClientType,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,

                // later activation flow
                PasswordHash = "",

                IsEmailVerified = false,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            _context.Clients.Add(client);

            await _context.SaveChangesAsync();

            foreach (var companyDto in dto.Companies)
            {
                var company = await _context.Companies
                    .FirstOrDefaultAsync(x =>
                        x.Name ==
                        companyDto.CompanyName);

                if (company == null)
                {
                    throw new Exception(
                        $"Company not found: " +
                        companyDto.CompanyName);
                }

                foreach (var branchDto
                    in companyDto.Branches)
                {
                    var branch = await _context.Branches
                        .FirstOrDefaultAsync(x =>
                            x.BranchCode ==
                            branchDto.BranchCode
                            &&
                            x.CompanyId ==
                            company.Id);

                    if (branch == null)
                    {
                        throw new Exception(
                            $"Branch not found: " +
                            branchDto.BranchCode);
                    }

                    var access =
                        new ClientCompanyBranchAccess
                        {
                            ClientId = client.Id,

                            CompanyId = company.Id,

                            BranchId = branch.Id,

                            CanLoadStarter =
                                branchDto.CanLoadStarter,

                            CanReviewTitle =
                                branchDto.CanReviewTitle,

                            CanIssueInsurance =
                                branchDto.CanIssueInsurance,

                            CreatedDate =
                                DateTime.UtcNow
                        };

                    _context
                        .ClientCompanyBranchAccesses
                        .Add(access);
                }
            }

            await _context.SaveChangesAsync();

            return client;
        }

        public async Task<Client> GetClientDetailsAsync(int clientID)
        {
            return await  _context.Clients.Include(x => x.ClientCompanyBranchAccesses).ThenInclude(x => x.Company).
                Include(x => x.ClientCompanyBranchAccesses).ThenInclude(x => x.Branch).ThenInclude
                (x => x.BranchStateCountyAccesses).ThenInclude(x => x.County).ThenInclude(x => x.State).FirstOrDefaultAsync(x => x.Id == clientID);
            
        
    }
    }
}