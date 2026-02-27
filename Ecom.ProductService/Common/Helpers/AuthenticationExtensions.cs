using Ecom.ProductService.Common.Requirement;
using Ecom.ProductService.Core.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace Ecom.ProductService.Common.Helpers
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddAuthenticationExtensions(this IServiceCollection services, IConfiguration configuration)
        {
            var _internalAuth = configuration
                 .GetSection("InternalAuth")
                 .Get<InternalAuth>()
                 ?? throw new InvalidOperationException("JwtSettings missing");
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = _internalAuth.Issuer;

                    // Chỉ để false khi đang ở môi trường Dev/Local không có SSL thật
                    options.RequireHttpsMetadata = false;

                    options.TokenValidationParameters = new TokenValidationParameters {
                        ValidateIssuer = true,
                        ValidIssuer = _internalAuth.Issuer,
                        ValidateAudience = true,
                        ValidAudience = _internalAuth.Audience,
                        ValidateLifetime = false,
                        ClockSkew = TimeSpan.FromSeconds(20),
                        ValidateIssuerSigningKey = true,

                    };
                });
            services.AddSingleton<IAuthorizationHandler, InternalOrPermissionHandler>();
            services.AddAuthorization(options =>
            {
                // Tất cả các Policy đều dùng chung Requirement, chỉ khác tham số Permission
                options.AddPolicy(PolicyNames.ProductRead, policy =>
                    policy.AddRequirements(new InternalOrPermissionRequirement("product.read")));
                options.AddPolicy(PolicyNames.ProductWrite, policy =>
                    policy.AddRequirements(new InternalOrPermissionRequirement("product.write")));
                // Nếu bạn vẫn muốn một Policy chỉ dành riêng cho internal (ví dụ các hàm admin hệ thống)
                options.AddPolicy(PolicyNames.Internal, policy =>
                {
                    policy.AddRequirements(new InternalOrPermissionRequirement("product.internal"));
                    policy.AddRequirements(new InternalOrPermissionRequirement("product.write"));
                    policy.AddRequirements(new InternalOrPermissionRequirement("product.read"));
                }

                 );
            });
            return services;
        }
    }
}
