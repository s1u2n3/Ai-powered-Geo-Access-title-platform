using Microsoft.Extensions.Caching.Distributed;
using NetTopologySuite.IO.Converters;
using System.Text.Json;
using WebApplication1.Services.Interfaces.RedisService;

namespace WebApplication1.Services.Implementaions.RedisService
{
    public class RedisService : IRedisService
    {
        private readonly IDistributedCache _cache;

        private readonly JsonSerializerOptions _jsonOptions;

        public RedisService(
            IDistributedCache cache)
        {
            _cache = cache;

            _jsonOptions =
                new JsonSerializerOptions();

            _jsonOptions.Converters.Add(
                new GeoJsonConverterFactory());
        }

        public async Task<T?> GetAsync<T>(
            string key)
        {
            var cachedValue =
                await _cache.GetStringAsync(key);

            if (string.IsNullOrEmpty(cachedValue))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(
                cachedValue,
                _jsonOptions);
        }

        public async Task SetAsync<T>(
            string key,
            T value,
            TimeSpan expiry)
        {
            var options =
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow =
                        expiry
                };

            var json =
                JsonSerializer.Serialize(
                    value,
                    _jsonOptions);

            await _cache.SetStringAsync(
                key,
                json,
                options);
        }

        public async Task RemoveAsync(
            string key)
        {
            await _cache.RemoveAsync(key);
        }
    }
}