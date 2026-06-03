using WebApplication1.Dto.PropertySearch;
using WebApplication1.Repositories.Interfaces;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Services.Implementaions
{
    public class GeoService:IGeoService
    {
        private  readonly IPropertyStarerRepository _repository;
        public GeoService(IPropertyStarerRepository repository)
        {
            _repository = repository;
        }

        public async Task<PropertyStaterDto>GetPropertyStaterAsync(string parcelId)
        {
            return await
                _repository
                    .GetParcelWithStarterAsync(parcelId);
        }
    }
}
