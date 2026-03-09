using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Ecom.ProductService.Common.Requirement
{
    public class GrpcApiKeyInterceptor : Interceptor
    {
        private readonly string _apiKey;

        public GrpcApiKeyInterceptor(IConfiguration configuration)
        {
            _apiKey = configuration["InternalGrpcApiKey"] ?? string.Empty ;
        }

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
    }
}
