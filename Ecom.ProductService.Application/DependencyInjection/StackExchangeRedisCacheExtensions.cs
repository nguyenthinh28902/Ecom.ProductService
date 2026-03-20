using Ecom.ProductService.Application.Interface;
using Ecom.ProductService.Application.Service;
using Ecom.ProductService.Core.Models.Connection.RedisConnection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Ecom.ProductService.Application.DependencyInjection
{
    public static class StackExchangeRedisCacheExtensions
    {
        public static IServiceCollection AddStackExchangeRedis(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RedisConnection>(configuration.GetSection(nameof(RedisConnection)));
            // 1. Mapping từ Section "RedisConnection" trong appsettings vào Model
            var redisSettings = configuration.GetSection(nameof(RedisConnection)).Get<RedisConnection>()
                ?? throw new InvalidOperationException("RedisConfig configuration is missing.");

            // 2. Đăng ký StackExchangeRedis với thông số từ Model
            services.AddStackExchangeRedisCache(options =>
            {
                var configOptions = ConfigurationOptions.Parse(redisSettings.RedisConnectionString);
                options.Configuration = redisSettings.RedisConnectionString;
                configOptions.ConnectTimeout = 1000;
                configOptions.SyncTimeout = 500;
                configOptions.AsyncTimeout = 500;
                configOptions.ConnectRetry = 0;
                configOptions.AbortOnConnectFail = false;

                options.ConfigurationOptions = configOptions;         
                options.InstanceName = redisSettings.InstanceName;
            });

            // Đăng ký Service xử lý cache user (như đã bàn ở bước trước)
            services.AddScoped<ICacheService, CacheService>();
            return services;
        }
    }
}
