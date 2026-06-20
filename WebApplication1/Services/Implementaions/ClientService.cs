using Azure;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System.Runtime.InteropServices;
using System.Security.Claims;
using WebApplication1.Dto;
using WebApplication1.Dto.Client;
using WebApplication1.entites;
using WebApplication1.Repositories.Implementations;
using WebApplication1.Repositories.Interfaces;
using WebApplication1.Services.Interfaces;
using WebApplication1.Services.Interfaces.RedisService;
using WebApplication1.MiddleWare.Exceptions;

namespace WebApplication1.Services.Implementations
{
    public class ClientService
        : IClientService
    {
        private readonly IClientRepository
            _repository;
        private readonly IConfiguration _configuration;
        private readonly IAuditLogsService _auditlogs;
        private readonly IRedisService _cache;


        public ClientService(IClientRepository repository ,IConfiguration configuration,IAuditLogsService auditlogs,IRedisService cache)
        {
            _repository = repository;
            _configuration=configuration;
            _auditlogs=auditlogs;
            _cache = cache;
        }

        public async Task<Client>
            CreateAsync(CreateClientDto dto ,String userId)
        {
            if (string.IsNullOrWhiteSpace(
                dto.ClientName))
            {
                throw new ValidationException(
                    "Client name is required");
            }

            if (string.IsNullOrWhiteSpace(
                dto.Email))
            {
                throw new ValidationException(
                    "Email is required");
            }


            var cleint = await _repository.CreateAsync(dto);
            await SendActivationEmailAsync(cleint.Email, cleint.ClientName, cleint.EmailVerificationToken);
            await _auditlogs.LogAsync("Create Client", "Client", cleint.Id.ToString(), $"Clint {cleint.ClientName} Created", userId);
            return cleint;
        }

        private async Task SendActivationEmailAsync(
     string email,
     string clientName,
     string token)
        {
            var host = _configuration["EmailSettings:Host"];
            var port = Convert.ToInt32(_configuration["EmailSettings:Port"]);

            var username =
                _configuration["EmailSettings:Username"];

            var password =
                _configuration["EmailSettings:Password"];

            var fromEmail =
                _configuration["EmailSettings:FromEmail"];

            var fromName =
                _configuration["EmailSettings:FromName"];

            var activationLink =
     $"http://geoaccesstitle.webhop.me/activate-account?token={token}";

            var message = new MimeMessage();

            message.From.Add(
                new MailboxAddress(
                    fromName,
                    fromEmail));

            message.To.Add(
                MailboxAddress.Parse(email));

            message.Subject =
                "Activate Your GeoAccess Account";

            message.Body =
                new TextPart("html")
                {
                    Text = $@"
                <h2>Welcome {clientName}</h2>

                <p>Your account has been created.</p>

                <p>
                    <a href='{activationLink}'>
                        Activate Account
                    </a>
                </p>

                <p>
                    This link expires in 24 hours.
                </p>"
                };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                host,
                port,
                SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(
                username,
                password);

            await smtp.SendAsync(message);

            await smtp.DisconnectAsync(true);
        }
        public async Task<ClientResponseDto> GetClientDetailsAsync(ClaimsPrincipal user)
        {
            // STEP 1
            // GET CLIENT ID
            // FROM JWT TOKEN

            var clientId =
      user.FindFirst(
          ClaimTypes.NameIdentifier)
      ?.Value;

            if (string.IsNullOrEmpty(clientId))
            {
                throw new UnauthorizedException (
                    "Invalid Client");
            }

            var cachedkey = $"client-details:{clientId}";
            var cachedData =  await _cache.GetAsync<ClientResponseDto>(cachedkey);

            if(cachedData != null)
            {
                return cachedData;
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

           var repsonse=new ClientResponseDto
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

            await _cache.SetAsync(cachedkey, repsonse, TimeSpan.FromDays(7));

            return repsonse;
        }
    }
}