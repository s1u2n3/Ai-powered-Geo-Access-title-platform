using WebApplication1.Dto;
using WebApplication1.Dto.Client;
using WebApplication1.entites;

namespace WebApplication1.Repositories.Interfaces
{
    public interface IClientRepository
    {
        Task<Client> CreateAsync(
            CreateClientDto dto);

        Task<Client> GetClientDetailsAsync(int clientid);
    }
}