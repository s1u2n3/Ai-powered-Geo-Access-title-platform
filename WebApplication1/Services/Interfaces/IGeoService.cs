using WebApplication1.Dto.PropertySearch;

namespace WebApplication1.Services.Interfaces
{
    public interface IGeoService
    {
        public Task<PropertyStaterDto> GetPropertyStaterAsync(string parcelId);
    }
}
