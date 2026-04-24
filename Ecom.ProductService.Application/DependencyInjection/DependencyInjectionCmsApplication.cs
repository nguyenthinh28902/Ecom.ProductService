using Ecom.ProductService.Application.Interface.Auth;
using Ecom.ProductService.Application.Interface.CMS;
using Ecom.ProductService.Application.Service.Auth;
using Ecom.ProductService.Application.Service.CMS;
using Ecom.ProductService.Application.Service.Dev;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Application.DependencyInjection
{
    public static class DependencyInjectionCmsApplication
    {
            public static IServiceCollection AddCmsApplication(this IServiceCollection services)
            {
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
            services.AddScoped<IBaseService, BaseService>();
            services.AddScoped<ISystemLogManagerService, SystemLogManagerService>();
            services.AddScoped<IProductSummaryService, ProductSummaryService>();
            services.AddScoped<IBrandManagerService, BrandManagerService>();
            services.AddScoped<IProductManagerService, ProductManagerService>();

            return services;
        }
    }
}
