using Ecom.ProductService.Application.Interface.Auth;
using Ecom.ProductService.Application.Interface.CMS;
using Ecom.ProductService.Application.Service.Auth;
using Ecom.ProductService.Application.Service.CMS;
using Ecom.ProductService.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecom.ProductService.Application.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependencyInjectionApplication(this IServiceCollection services,
         IConfiguration configuration)
        {
            services.AddDependencyInjectionInfrastructure(configuration);
            services.AddStackExchangeRedis(configuration);
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IBaseService, BaseService>();
            services.AddScoped<IProductSummaryService, ProductSummaryService>();
            return services;
        }
    }
}
