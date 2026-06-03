using WebApplication1.Dto.PropertySearch;

namespace WebApplication1.Repositories.Interfaces
{
    public interface IPropertyStarerRepository
    {
        Task<PropertyStaterDto> GetParcelWithStarterAsync(string parcelId);

    }
}
