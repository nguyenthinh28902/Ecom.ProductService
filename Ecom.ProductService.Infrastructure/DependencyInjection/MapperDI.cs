using Ecom.ProductService.Core.AutoMappings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Infrastructure.DependencyInjection
{
    public static class MapperDI
    {
        public static IServiceCollection AddMapperDI(this IServiceCollection services) {

            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<ApplicationNavigationProfile>();
                cfg.AddProfile<ApplicationProductWebProfile>();
            });
            return services; 
        }
    }
}
