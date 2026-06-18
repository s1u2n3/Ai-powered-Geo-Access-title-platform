using WebApplication1.Dto.PropertySearch;
using WebApplication1.Repositories.Interfaces;
using WebApplication1.Services.Interfaces;
using WebApplication1.Services.Interfaces.RedisService;

namespace WebApplication1.Services.Implementaions
{
    public class GeoService:IGeoService
    {
        private  readonly IPropertyStarerRepository _repository;
        private readonly IRedisService _cache;
        public GeoService(IPropertyStarerRepository repository,IRedisService cache)
        {
            _repository = repository;
            _cache = cache;

        }

        public async Task<PropertyStaterDto>GetPropertyStaterAsync(string parcelId)
        {
            var cacheKey = $"parcel-starter:{parcelId}";
            var cachedData = await _cache.GetAsync<PropertyStaterDto>(cacheKey);
            if(cachedData != null)
            {
                return cachedData;
            }
            var response= await
                _repository
                    .GetParcelWithStarterAsync(parcelId);
           

            await _cache.SetAsync(
                cacheKey,
                response,TimeSpan.FromDays(7));

            return response;
        }
    }
}
