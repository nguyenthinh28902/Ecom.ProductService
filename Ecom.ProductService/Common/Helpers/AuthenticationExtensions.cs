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
            var _internalAuthWeb = configuration
                 .GetSection("InternalAuthWeb")
                 .Get<InternalAuth>()
                 ?? throw new InvalidOperationException("JwtSettings missing");
            services.AddAuthentication(options =>
            {
                // Sử dụng DefaultAuthenticateScheme chung để Middleware tự động kiểm tra cả hai
                options.DefaultAuthenticateScheme = "Bearer";
                options.DefaultChallengeScheme = "WebScheme";
            }).AddJwtBearer("Bearer", options =>
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
                }).AddJwtBearer("WebScheme", options => // Scheme cho nguồn Web (localhost:7109)
                {
                    options.Authority = _internalAuthWeb.Issuer;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = _internalAuthWeb.Issuer,
                        ValidateAudience = true,
                        ValidAudience = _internalAuthWeb.Audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromSeconds(20),
                        ValidateIssuerSigningKey = true,
                    };
                });
            services.AddSingleton<IAuthorizationHandler, InternalOrPermissionHandler>();
            services.AddAuthorization(options =>
            {
                // 1. Quyền Web: Chỉ dành cho WebScheme
                options.AddPolicy(PolicyNames.ProductReadWeb, policy =>
                {
                    policy.AddAuthenticationSchemes("WebScheme");
                    policy.AddRequirements(new InternalOrPermissionRequirement("product.read.web"));
                });

                // 2. Các quyền hệ thống: Chỉ dành cho Bearer (Internal)
                options.AddPolicy(PolicyNames.ProductRead, policy =>
                {
                    policy.AddAuthenticationSchemes("Bearer");
                    policy.AddRequirements(new InternalOrPermissionRequirement("product.read"));
                });

                options.AddPolicy(PolicyNames.ProductWrite, policy =>
                {
                    policy.AddAuthenticationSchemes("Bearer");
                    policy.AddRequirements(new InternalOrPermissionRequirement("product.write"));
                });

                // 3. Policy Internal: Bao gồm tất cả các nguồn và tất cả các quyền
                options.AddPolicy(PolicyNames.Internal, policy =>
                {
                    // Cho phép cả 2 Scheme để Admin từ nguồn nào cũng có thể truy cập nếu đủ quyền
                    policy.AddAuthenticationSchemes("Bearer");

                    // Yêu cầu đầy đủ các quyền (bao gồm cả quyền .web như bạn mong muốn)
                    policy.AddRequirements(new InternalOrPermissionRequirement("product.internal"));
                    policy.AddRequirements(new InternalOrPermissionRequirement("product.write"));
                    policy.AddRequirements(new InternalOrPermissionRequirement("product.read"));
                    policy.AddRequirements(new InternalOrPermissionRequirement("product.read.web"));
                });
            });
            return services;
        }
    }
}
