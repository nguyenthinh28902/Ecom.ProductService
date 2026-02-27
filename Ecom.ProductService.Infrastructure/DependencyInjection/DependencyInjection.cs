using Ecom.ProductService.Core.Abstractions.Persistence;
using Ecom.ProductService.Core.Models.Db;
using Ecom.ProductService.Infrastructure.DbContexts;
using Ecom.ProductService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecom.ProductService.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependencyInjectionInfrastructure(this IServiceCollection services,
         IConfiguration configuration)
        {
            ConnectionStrings.ProductDb = configuration.GetConnectionString("EcommerceProduct") ?? string.Empty;
            // Đăng ký DbContext sử dụng SQL Server
            services.AddDbContext<EcomProductDbContext>(options =>
                options.UseSqlServer(ConnectionStrings.ProductDb));

            //add kiến trúc repo and UoW
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            return services;
        }
    }
}
