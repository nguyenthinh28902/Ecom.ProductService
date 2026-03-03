using Ecom.ProductService.Application.Interface.CMS;
using Ecom.ProductService.Application.Interface.Web;
using Ecom.ProductService.Application.Service.CMS;
using Ecom.ProductService.Application.Service.Web;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Application.DependencyInjection
{
    public static class DependencyInjectionWebApplication
    {
        public static IServiceCollection AddWebApplication(this IServiceCollection services)
        {
            services.AddScoped<IProductWebService, ProductWebService>();
            services.AddScoped<INavigationService, NavigationService>();
            return services;
        }
    }
}
