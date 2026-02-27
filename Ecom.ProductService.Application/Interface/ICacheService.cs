namespace Ecom.ProductService.Application.Interface
{
    public interface ICacheService
    {
        Task SetAsync<T>(string key, T value, int expirationMinutes) where T : class;
        Task<T?> GetAsync<T>(string key) where T : class;
        Task RemoveAsync(string key);
    }
}
