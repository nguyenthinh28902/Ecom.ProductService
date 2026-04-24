using Ecom.ProductService.Application.Interface.Auth;
using Ecom.ProductService.Application.Interface.CMS;
using Ecom.ProductService.Application.Interface.Web;
using Ecom.ProductService.Application.Service.Auth;
using Ecom.ProductService.Application.Service.CMS;
using Ecom.ProductService.Application.Service.Dev;
using Ecom.ProductService.Application.Service.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Application.DependencyInjection
{
    public static class DependencyInjectionWebApplication
    {
        public static IServiceCollection AddWebApplication(this IServiceCollection services)
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == Environments.Development)
            {
                services.AddScoped<ICurrentCustomerService, CurrentCustomerServiceDev>();
            }
            else
            {
                services.AddScoped<ICurrentCustomerService, CurrentCustomerService>();
            }
            services.AddScoped<IProductWebService, ProductWebService>();
            services.AddScoped<INavigationService, NavigationService>();
            return services;
        }
    }
}
