using Ecom.ProductService.Application.DependencyInjection;

namespace Ecom.ProductService.Common.DependencyInjection
{
    public static class ApplicationDI
    {
        public static IServiceCollection AddApplicationDI(
            this IServiceCollection services,
            IConfiguration configuration,
            ILoggerFactory loggerFactory)
        {
            services.AddDependencyInjectionApplication(configuration, loggerFactory);
            return services;
        }
    }
}
