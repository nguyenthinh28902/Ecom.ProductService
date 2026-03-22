using Ecom.ProductService.Application.AutoMappings;
using Ecom.ProductService.Application.Interface.Auth;
using Ecom.ProductService.Application.Interface.CMS;
using Ecom.ProductService.Application.Service.Auth;
using Ecom.ProductService.Application.Service.CMS;
using Ecom.ProductService.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ecom.ProductService.Application.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependencyInjectionApplication(this IServiceCollection services,
         IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            services.AddDependencyInjectionInfrastructure(configuration);
            services.AddStackExchangeRedis(configuration);
                services.AddAutoMapper(cfg =>
                {
                    cfg.AddProfile<ApplicationNavigationProfile>();
                    cfg.AddProfile<ApplicationProductWebProfile>();
                });
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<ICurrentCustomerService, CurrentCustomerService>();
            services.AddScoped<IBaseService, BaseService>();
            services.AddCmsApplication();
            services.AddWebApplication();
            return services;
        }
    }
}
