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
                // 1. Parse connection string vào Object cấu hình
                var configOptions = ConfigurationOptions.Parse(redisSettings.RedisConnectionString);

                // 2. Cấu hình Timeout siêu ngắn để tránh treo Thread
                configOptions.ConnectTimeout = 100;     // 100ms để kết nối (Mặc định: 5000ms)
                configOptions.SyncTimeout = 100;        // 100ms cho lệnh đồng bộ (Mặc định: 5000ms)
                configOptions.AsyncTimeout = 100;       // 100ms cho lệnh bất đồng bộ (Mặc định: 5000ms)

                // 3. Quản lý việc kết nối lại khi có sự cố
                configOptions.ConnectRetry = 1;         // Thử lại tối đa 1 lần nếu mất kết nối (Tránh loop vô hạn)
                configOptions.AbortOnConnectFail = false; // QUAN TRỌNG: Redis chết ứng dụng vẫn khởi động bình thường

                // 4. Gán Object cấu hình vào options (KHÔNG set options.Configuration nữa)
                options.ConfigurationOptions = configOptions;
                options.InstanceName = redisSettings.InstanceName;
            });

            // Đăng ký Service xử lý cache user (như đã bàn ở bước trước)
            services.AddScoped<ICacheService, CacheService>();
            return services;
        }
    }
}
