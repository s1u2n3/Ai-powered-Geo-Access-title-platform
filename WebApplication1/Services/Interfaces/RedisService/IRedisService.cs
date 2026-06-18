namespace WebApplication1.Services.Interfaces.RedisService
{
    public interface IRedisService
    {
        Task<T> GetAsync<T>(string key);

        Task SetAsync<T>(string key, T Value, TimeSpan expiry);

        Task RemoveAsync(string key);

    }
}
