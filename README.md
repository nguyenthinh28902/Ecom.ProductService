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
## Kiến trúc dự án
### Mô hình
- Mô hình 3 layer.
- [Unit of work và Repository](https://github.com/nguyenthinh28902/Ecom.ProductService/tree/main/Ecom.ProductService.Infrastructure/Repositories)
+ Unit of work 
```csharp
        private readonly EcomProductDbContext dbContext;
        private Dictionary<Type, object> _repositories = new Dictionary<Type, object>();
        private IDbContextTransaction _transaction;
        public UnitOfWork(EcomProductDbContext context)
        {
            dbContext = context;
        }

        public IRepository<T> Repository<T>() where T : class
        {
            IRepository<T> repository = null;
            if (_repositories.ContainsKey(typeof(T)))
            {
                repository = _repositories[typeof(T)] as IRepository<T>;
            }
            else
            {
                repository = new Repository<T>(dbContext);
                _repositories.Add(typeof(T), repository);
            }

            return (Repository<T>)repository;
        }
```
+ Repository
```csharp
        private EcomProductDbContext _context;
        public Repository(EcomProductDbContext _context)
        {
            this._context = _context;
        }

        /// <summary>
        /// add 1 item
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }
```
+ Code mẫu
```csharp
 try
 {
     // 1. Lấy hoặc khởi tạo giỏ hàng cho khách hàng
     var cart = await _unitOfWork.Repository<Cart>()
         .GetAll(x => x.CustomerId == customerId)
         .Include(x => x.CartItems)
         .FirstOrDefaultAsync();

     if (cart == null)
     {
         // Nếu chưa có giỏ hàng thì tạo mới
         cart = new Cart
         {
             CustomerId = customerId,
             CreatedAt = DateTime.UtcNow
         };
         await _unitOfWork.Repository<Cart>().AddAsync(cart);
         await _unitOfWork.SaveChangesAsync();
         if(cart.Id == 0) return Result<bool>.Failure("Có lỗi xảy ra khi thêm vào giỏ hàng");
         // Lưu để có ID giỏ hàng trước khi thêm item (tùy thuộc vào thiết kế DB)
         // Hoặc để EF Core tự xử lý quan hệ nếu CartId là FK
     }

     await _unitOfWork.BeginTransactionAsync();
     // 2. Kiểm tra sản phẩm (với variant cụ thể) đã tồn tại trong giỏ chưa
     var existingItem = cart.CartItems
         .FirstOrDefault(x => x.ProductId == request.ProductId && x.VariantId == request.VariantId);
     // Chỉ comment dòng quan trọng: Log trạng thái giỏ hàng để kiểm soát luồng thêm mới hoặc cập nhật
     _logger.LogInformation("Check cart customer: {CustomerId} | Status: {CartStatus}",
         customerId,
         existingItem != null ? "Existing Cart" : "New Cart");
     if (existingItem != null)
     {
         // Nếu đã có: Cập nhật thêm số lượng
         existingItem.Quantity += request.Quantity;
         existingItem.AddedAt = DateTime.UtcNow; // Cập nhật lại thời gian tương tác gần nhất

         _unitOfWork.Repository<CartItem>().Update(existingItem);
     }
     else
     {
         // Nếu chưa có: Thêm mới item vào giỏ
         var newItem = new CartItem
         {
             CartId = cart.Id,
             ProductId = request.ProductId,
             VariantId = request.VariantId,
             Quantity = request.Quantity,
             AddedAt = DateTime.UtcNow
         };
         await _unitOfWork.Repository<CartItem>().AddAsync(newItem);
     }

     // 3. Cập nhật thời gian thay đổi của giỏ hàng
     cart.UpdatedAt = DateTime.UtcNow;
     _unitOfWork.Repository<Cart>().Update(cart);

     // 4. Xác nhận lưu toàn bộ thay đổi xuống Database
     await _unitOfWork.CommitAsync();

     _logger.LogInformation("Product {ProductId} added to cart successfully for customer {CustomerId}",
         request.ProductId, customerId);

     return Result<bool>.Success(true, "Đã thêm sản phẩm vào giỏ hàng thành công");
 }
 catch (Exception ex)
 {
     _logger.LogError(ex, "Error adding product to cart for customer {CustomerId}", customerId);
     return Result<bool>.Failure("Có lỗi xảy ra khi thêm vào giỏ hàng");
 }
```
### Tích hợp Grpc
- Bảo mật. []()
```csharp
           return builder.AddCallCredentials(async (context, metadata, serviceProvider) =>
           {
               var currentCustomer = serviceProvider.GetRequiredService<ICurrentCustomerService>();
               var currentUserService = serviceProvider.GetRequiredService<ICurrentUserService>();

               // 1. Chỉ comment dòng quan trọng: Tự động đính kèm thông tin user nếu đã login
               if (currentUserService.IsAuthenticated || currentCustomer.IsAuthenticated)
               {

                   if (!currentUserService.IsAuthenticated)
                   {
                       metadata.Add("X-User-Id", currentCustomer.Id.ToString());
                       if (!string.IsNullOrEmpty(currentCustomer.Email))
                           metadata.Add("X-User-Email", currentCustomer.Email);
                       if (!string.IsNullOrEmpty(currentCustomer.PhoneNumber))
                           metadata.Add("X-User-Phone", currentCustomer.PhoneNumber);
                   }
                   else
                   {
                       metadata.Add("X-User-Id", currentUserService.UserId.ToString());
                       metadata.Add("X-User-WorkplaceId", currentUserService.WorkplaceId.ToString());
                       if (!string.IsNullOrEmpty(currentUserService.Email))
                       metadata.Add("X-User-Email", currentUserService.Email.ToString());
                       if (!string.IsNullOrEmpty(currentUserService.WorkplaceType))
                       metadata.Add("X-User-WorkplaceType", currentUserService.WorkplaceType.ToString());
                       if (currentUserService.Roles.Count() > 0)
                       {
                           var rolesString = string.Join(",", currentUserService.Roles);
                           metadata.Add("X-User-Roles", rolesString);
                       }
                       if(!string.IsNullOrEmpty(currentUserService.WorkplaceType)) metadata.Add("X-User-WorkplaceType",                                                                         currentUserService.WorkplaceType.ToString());
                       if (currentUserService.Scopes.Count() > 0)
                       {
                           var scopesString = string.Join(",", currentUserService.Scopes);
                           metadata.Add("X-User-Scopes", scopesString);
                       }
                   }
               }
               // 2. Chỉ comment dòng quan trọng: Đính kèm API Key nội bộ lấy từ file cấu hình
               var internalKey = configuration["InternalGrpcApiKey"] ?? string.Empty;
               metadata.Add("x-internal-key", internalKey);

               await Task.CompletedTask;
           });
```
- Cấu hình. []()
 + Client.
   
 . Đăng ký. [DependencyInjectionWebApplication.cs](https://github.com/nguyenthinh28902/ecom-order-service/blob/main/Ecom.OrderService.Application/DependencyInjection/DependencyInjectionWebApplication.cs)
```csharp
   services.AddGrpcClient<ProductGrpc.ProductGrpcClient>(o => o.Address = new Uri(productUrl))
.AddCommonCallCredentials(configuration);

   // Đăng ký Payment Service Client
   services.AddGrpcClient<PaymentGrpc.PaymentGrpcClient>(o => o.Address = new Uri(paymentUrl))
           .AddCommonCallCredentials(configuration);
```
. Cấu hình poto file [payment.proto](https://github.com/nguyenthinh28902/ecom-order-service/blob/main/Ecom.OrderService.Application/Protos/payment.proto)
```csharp
syntax = "proto3";

// 1. Chỉ comment dòng quan trọng: Import thư viện thời gian chuẩn của Google
import "google/protobuf/timestamp.proto";

option csharp_namespace = "Ecom.PaymentService.Grpc";
package payment;

service PaymentGrpc {
  rpc ProcessPayment (PaymentGrpcRequest) returns (PaymentGrpcResponse);
  rpc GetTransactionByOrderId (GetTransactionRequest) returns (TransactionGrpcResponse);
  rpc GetTransactionByOrderIdManager (OrderTransactionGrpcRequest) returns (TransactionManagerGrpcResponse);
}
// cấu hình khác

```
. Code [OrderWebService.cs](https://github.com/nguyenthinh28902/ecom-order-service/blob/main/Ecom.OrderService.Application/Service/Web/OrderWebService.cs)
```csharp
                var paymentGrpcRequest = new PaymentGrpcRequest();
                paymentGrpcRequest.Amount = (double)order.TotalAmount;
                paymentGrpcRequest.Currency = order.Currency;
                paymentGrpcRequest.OrderId = order.Id;
                paymentGrpcRequest.OrderCode = order.OrderCode;
               
                paymentGrpcRequest.Description = $"Thanh toán cho đơn hàng {order.OrderCode}";
                paymentGrpcRequest.PaymentMethodCode = request.PaymentMethodCode;
                var paymentResult = await _paymentGrpcClient.ProcessPaymentAsync(paymentGrpcRequest);
                
```
+ Server
. Bảo mật [GrpcApiKeyInterceptor.cs](https://github.com/nguyenthinh28902/ecom-payment/blob/main/Ecom.PaymentService.Api/Common/Requirement/GrpcApiKeyInterceptor.cs)
```csharp
             public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
                    {
                        // Chỉ comment dòng quan trọng: Lấy Key từ Header "x-api-key"
                        var headerKey = context.RequestHeaders.GetValue("x-internal-key");
            
                        if (headerKey != _apiKey)
                        {
                            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Internal API Key"));
                        }
            
                        return await continuation(request, context);
                    }
```
. Code [OrderPaymentGrpc.cs](https://github.com/nguyenthinh28902/ecom-payment/blob/main/Ecom.PaymentService.Api/Controller/Web/OrderPaymentGrpc.cs)
```csharp
public override async Task<PaymentGrpcResponse> ProcessPayment(PaymentGrpcRequest request, ServerCallContext context)
        {
            var dto = new OrderRequestDto
            {
                OrderId = request.OrderId,
                OrderCode = request.OrderCode,
                Amount = (decimal)request.Amount,
                Currency = request.Currency,
                PaymentMethodCode = request.PaymentMethodCode,
                Description = request.Description
            };

            var result = await _paymentWebService.ProcessPaymentAsync(dto);

            return new PaymentGrpcResponse
            {
                IsSuccess = result.IsSuccess,
                Message = result.Noti,
                ApprovalUrl = result.Data?.ApprovalUrl ?? "",
                OrderCode = result.Data?.OrderCode ?? ""
            };
        }
```
### Tích hợp RabbitMQ
- Cấu hình. [RabbitMQInfrastructure.cs](https://github.com/nguyenthinh28902/ecom-notification-service/blob/main/Ecom.Notification.Application/DependencyInjection/RabbitMQInfrastructure.cs)
```csharp
            services.AddMassTransit(x =>
            {
                // Đăng ký class xử lý logic khi có tin nhắn đến
                x.AddConsumers(typeof(NotificationConsumer).Assembly);
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitSettings.Host, (ushort)rabbitSettings.Port, "/", h =>
                    {
                        h.Username(rabbitSettings.UserName);
                        h.Password(rabbitSettings.Password);
                    });

                    // Cấu hình Endpoint để lắng nghe Queue cụ thể
                    cfg.ConfigureEndpoints(context);
                });
            });
```
- Code mẫu

[NotificationConsumer.cs](https://github.com/nguyenthinh28902/ecom-notification-service/blob/main/Ecom.Notification.Application/Service/Consumer/NotificationConsumer.cs)
```csharp
public async Task Consume(ConsumeContext<NotificationRequestDto> context)
        {
            var eventData = context.Message;
            try
            {
                await _notificationService.DispatchNotificationAsync(eventData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý tin nhắn từ RabbitMQ cho {Email}", eventData.ReceiverEmail);
                // Ném lỗi để MassTransit thực hiện Retry (thử lại) nếu đã cấu hình
                throw;
            }
        }
    }
```
[NotificationConsumerDefinition.cs](https://github.com/nguyenthinh28902/ecom-notification-service/blob/main/Ecom.Notification.Application/Service/ConsumerDefinition/NotificationConsumerDefinition.cs)
```csharp
 protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<NotificationConsumer> consumerConfigurator, IRegistrationContext context)
        {
            // Cấu hình Retry riêng cho ông này
            endpointConfigurator.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
        }
```
[OrderWebService.cs](https://github.com/nguyenthinh28902/ecom-order-service/blob/main/Ecom.OrderService.Application/Service/Web/OrderWebService.cs)
```csharp
 
        private readonly IPublishEndpoint _publishEndpoint;
        public OrderWebService(
            IPublishEndpoint publishEndpoint)
        {
           
            _publishEndpoint = publishEndpoint;
        }
```
```csharp
            var notificationEvent = new NotificationRequestDto
            {
                        ReceiverId = customerId,
                        ReceiverRole = "CUSTOMER",
                        ReceiverEmail = _currentCustomerService.Email,
                        TypeCode = "ORDER_SUCCESS_CUSTOMER",
                        Channel = "EMAIL",
                        Message = $"Thông báo đơn hàng {order.OrderCode}",
                        Parameters = new Dictionary<string, string>
                        {
                            { "customer_name", order.FullName },
                            { "order_code", order.OrderCode },
                            { "total_amount", $"{order.TotalAmount:N0} {order.Currency}" },
                            
                        },
                                Items = order.OrderItems.Select(x => new Dictionary<string, string>
                        {
                            { "product_name", x.ProductName },
                            { "quantity", x.Quantity.ToString() },
                            { "sub_total", $"{(x.UnitPrice * x.Quantity):N0} {order.Currency}" }
                        }).ToList()
            };

            await _publishEndpoint.Publish(notificationEvent);
```
