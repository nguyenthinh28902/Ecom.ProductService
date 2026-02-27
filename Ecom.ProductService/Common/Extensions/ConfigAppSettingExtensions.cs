using Ecom.ProductService.Core.Models.Auth;
using Ecom.ProductService.Core.Models.Connection.RedisConnection;

namespace Ecom.ProductService.Common.Extensions
{
    public static class ConfigAppSettingExtensions
    {
        public static IServiceCollection AddConfigAppSettingExtensions(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<InternalAuth>(configuration.GetSection("InternalAuth"));
            services.Configure<RedisConnection>(configuration.GetSection("RedisConnection"));
            return services;
        }
    }
}
