using Ecom.ProductService.Core.Abstractions.Persistence;
using Ecom.ProductService.Core.Abstractions.Persistence.ReadOnly;
using Ecom.ProductService.Core.Models.Db;
using Ecom.ProductService.Infrastructure.DbContexts;
using Ecom.ProductService.Infrastructure.Repositories;
using Ecom.ProductService.Infrastructure.Repositories.ReadOnly;
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
            ConnectionStrings.ProductMasterDb = configuration.GetConnectionString("EcommerceProduct") ?? string.Empty;
            ConnectionStrings.ProductReplicationDb = configuration.GetConnectionString("EcommerceProductReplication") ?? string.Empty;
            // Đăng ký DbContext sử dụng SQL Server
            services.AddDbContext<EcomProductDbContext>(options =>
                options.UseSqlServer(ConnectionStrings.ProductMasterDb));
            services.AddDbContext<ReadOnlyDbContext>(options =>
                options.UseSqlServer(ConnectionStrings.ProductReplicationDb));


            //add kiến trúc repo and UoW
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IReadOnlyUnitOfWork, ReadOnlyUnitOfWork>(sp => {
                var context = sp.GetRequiredService<ReadOnlyDbContext>();
                var serviceProvider = sp.GetRequiredService<IServiceProvider>();
                return new ReadOnlyUnitOfWork(context, serviceProvider); // UnitOfWork này sẽ dùng ReplicaDb
            });
            //add kiến trúc read only repo
            services.AddMapperDI();

            //serivce repo
            services.AddScoped<IBrandReadOnlyRepository, BrandReadOnlyRepository>();
            services.AddScoped<ICategoryReadOnlyRepository, CategoryReadOnlyRepository>();

            services.AddScoped<IProductSummaryReadOnlyRepository, ProductSummaryReadOnlyRepository>();
            services.AddScoped<IProductReadOnlyRepository, ProductReadOnlyRepository>();
            services.AddScoped<IOrderReadOnlyRepository, OrderReadOnlyRepository>();
            return services;
        }
    }
}
