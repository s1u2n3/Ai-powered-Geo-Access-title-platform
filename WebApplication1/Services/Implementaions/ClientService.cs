using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.Dto;
using WebApplication1.Dto.Client;
using WebApplication1.entites;
using WebApplication1.Repositories.Implementations;
using WebApplication1.Repositories.Interfaces;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Services.Implementations
{
    public class ClientService
        : IClientService
    {
        private readonly IClientRepository
            _repository;

        public ClientService(
            IClientRepository repository)
        {
            _repository = repository;
        }

        public async Task<Client>
            CreateAsync(CreateClientDto dto)
        {
            if (string.IsNullOrWhiteSpace(
                dto.ClientName))
            {
                throw new Exception(
                    "Client name is required");
            }

            if (string.IsNullOrWhiteSpace(
                dto.Email))
            {
                throw new Exception(
                    "Email is required");
            }

            return await
                _repository.CreateAsync(dto);
        }
        public async Task<ClientResponseDto> GetClientDetailsAsync(ClaimsPrincipal user)
        {
            // STEP 1
            // GET CLIENT ID
            // FROM JWT TOKEN

            var clientId =
                user.FindFirst("ClientId")
                ?.Value;

            if (string.IsNullOrEmpty(clientId))
            {
                throw new Exception(
                    "Invalid Client");
            }

            // STEP 2
            // FETCH CLIENT

            var client =
                await _repository
                .GetClientDetailsAsync(
                    int.Parse(clientId));

            if (client == null)
            {
                throw new Exception(
                    "Client not found");
            }

            // STEP 3
            // GROUP BY COMPANY

            var companies =
                client
                .ClientCompanyBranchAccesses

                .GroupBy(x => new
                {
                    x.CompanyId,
                    x.Company.Name,


                })

                .Select(companyGroup =>
                    new ClientCompanyResponseDto
                    {
                        CompanyId =
                            companyGroup.Key.CompanyId,
                        CompanyName = companyGroup.Key.Name,



                        Branches =
                            companyGroup
                            .Select(x =>
                                new ClientBranchResponseDto
                                {
                                    BranchId =
                                        x.BranchId,

                                    BranchCode =
                                        x.Branch.BranchCode,

                                    BranchName =
                                        x.Branch.BranchName,

                                    BranchType =
                                        x.Branch.BranchType,

                                    CanLoadStarter =
                                        x.CanLoadStarter,

                                    CanReviewTitle =
                                        x.CanReviewTitle,

                                    CanIssueInsurance =
                                        x.CanIssueInsurance,

                                    GeoAccess =
                                        x.Branch
                                        .BranchStateCountyAccesses

                                        .GroupBy(g =>
                                            new
                                            {
                                                g.County.State.StateCode,

                                                g.County.State.StateName,

                                                g.County.State.Latitude,

                                                g.County.State.Longitude
                                            })

                                        .Select(stateGroup =>
                                            new StateAndCountyResponse
                                            {
                                                StateCode =
                                                    stateGroup.Key.StateCode,

                                                StateName =
                                                    stateGroup.Key.StateName,

                                                StateLatitude =
                                                    stateGroup.Key.Latitude,

                                                StateLongitude =
                                                    stateGroup.Key.Longitude,

                                                Counties =
                                                    stateGroup
                                                    .Select(c =>
                                                        new CountyItemDto
                                                        {
                                                            CountyCode =
                                                                c.County.CountyCode,

                                                            CountyName =
                                                                c.County.CountyName,

                                                            CountyLatitude =
                                                                c.County.Latitude,

                                                            CountyLongitude =
                                                                c.County.Longitude
                                                        })
                                                    .ToList()
                                            })
                                        .ToList()
                                })
                            .ToList()
                    })
                .ToList();

            // STEP 4
            // FINAL RESPONSE

            return new
                ClientResponseDto
            {
                ClientId = client.Id,

                ClientCode =
                    client.ClientCode,

                ClientName =
                    client.ClientName,

                ClientType =
                    client.ClientType,

                Email =
                    client.Email,

                PhoneNumber =
                    client.PhoneNumber,

                Companies = companies
            };
        }
    }
}