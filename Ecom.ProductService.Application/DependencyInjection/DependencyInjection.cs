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
            
            
            
            services.AddRabbitMQExtension(configuration);
            services.AddCmsApplication();
            services.AddWebApplication();
            return services;
        }
    }
}
