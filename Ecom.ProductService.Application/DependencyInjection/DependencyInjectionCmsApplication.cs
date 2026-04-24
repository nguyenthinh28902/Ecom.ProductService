using Ecom.ProductService.Application.Interface.CMS;
using Ecom.ProductService.Application.Service.CMS;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Application.DependencyInjection
{
    public static class DependencyInjectionCmsApplication
    {
            public static IServiceCollection AddCmsApplication(this IServiceCollection services)
            {

            services.AddScoped<ISystemLogManagerService, SystemLogManagerService>();
            services.AddScoped<IProductSummaryService, ProductSummaryService>();
            services.AddScoped<IBrandManagerService, BrandManagerService>();
            services.AddScoped<IProductManagerService, ProductManagerService>();

            return services;
        }
    }
}
