using System.Security.Claims;
using WebApplication1.Dto;
using WebApplication1.Dto.Client;
using WebApplication1.entites;

namespace WebApplication1.Services.Interfaces
{
    public interface IClientService
    {
        Task<Client> CreateAsync(
            CreateClientDto dto);

        Task<ClientResponseDto> GetClientDetailsAsync(ClaimsPrincipal user);
    }

    
}