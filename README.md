# Giao diện quản lý 
## Giới thiệu
- Dịch vụ xử lý nghiệp vụ cho thương mại điện tử. phiên bản readme.md này được dùng chung cho các project service khác.
## 🛠 Công nghệ
- **Framework:** .NET Api Core, Grpc, RabbitMQ.
- **Giao thức:** OpenID Connect (OIDC) & OAuth.
- **Khác:** Memory cache.

## 🔄 Workflow (Luồng xác thực)﻿
### Cấu hình xác thực tại Web
[Xem tiếp](https://github.com/nguyenthinh28902/ecommerce-cms-web).

### Xác thực tại identity
[Xem tiếp](https://github.com/nguyenthinh28902/ecommerce-identity-server-cms).

### Xác thực tại Getaway 
[Xem tiếp](https://github.com/nguyenthinh28902/ecommerce-api-gateway-cms).

### Xác thực tại Service (Product servcie)
- Thực hiện xác thực token tử 2 nguồn. [AuthenticationExtensions.cs](https://github.com/nguyenthinh28902/Ecom.ProductService/blob/main/Ecom.ProductService/Common/Helpers/AuthenticationExtensions.cs)
```csharp
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
```
- Kiểm tra quyền. [AuthenticationExtensions.cs](https://github.com/nguyenthinh28902/Ecom.ProductService/blob/main/Ecom.ProductService/Common/Helpers/AuthenticationExtensions.cs)
```csharp
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
```
- Chỉ cho phép token hệ thống đi qua. [InternalOrPermissionHandler.cs](https://github.com/nguyenthinh28902/Ecom.ProductService/blob/main/Ecom.ProductService/Common/Helpers/InternalOrPermissionHandler.cs)
```csharp
           // Kiểm tra nếu Token có claim 'client_id' nhưng KHÔNG có 'sub' (User ID)
            // Đây là dấu hiệu của Client Credentials Flow (System-to-System)
            var isSystemToken = context.User.HasClaim(c => c.Type == "client_id")
                        && !context.User.HasClaim(c => c.Type == "sub");
            // Kiểm tra quyền cụ thể được truyền vào policy (ví dụ: user.internal)
            var hasPermission = context.User.HasClaim(c => c.Value == requirement.RequiredPermission);

            if (isSystemToken || hasPermission)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
```
