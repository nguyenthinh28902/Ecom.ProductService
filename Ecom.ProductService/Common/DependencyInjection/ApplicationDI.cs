using Ecom.ProductService.Application.DependencyInjection;

namespace Ecom.ProductService.Common.DependencyInjection
{
    public static class ApplicationDI
    {
        public static IServiceCollection AddApplicationDI(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDependencyInjectionApplication(configuration);
            return services;
        }
    }
}
