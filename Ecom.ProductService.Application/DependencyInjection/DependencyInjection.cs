using Ecom.ProductService.Application.Interface.Auth;
using Ecom.ProductService.Application.Service.Auth;
using Ecom.ProductService.Application.Service.Dev;
using Ecom.ProductService.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ecom.ProductService.Application.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependencyInjectionApplication(this IServiceCollection services,
         IConfiguration configuration)
        {
            services.AddDependencyInjectionInfrastructure(configuration);
            services.AddStackExchangeRedis(configuration);
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == Environments.Development)
            {
                // Khi chạy Local, dùng bản Dev này để không cần Token
                services.AddScoped<ICurrentUserService, CurrentUserServiceDev>();
            }
            else
            {
                // Khi lên Staging/Production, dùng bản thật đọc Header từ Gateway
                services.AddScoped<ICurrentUserService, CurrentUserService>();
            }
            services.AddScoped<ICurrentCustomerService, CurrentCustomerService>();
            services.AddScoped<IBaseService, BaseService>();
            services.AddCmsApplication();
            services.AddWebApplication();
            return services;
        }
    }
}
