using Ecom.ProductService.Core.Exceptions;
using System.Net;
using System.Text.Json;

namespace Ecom.ProductService.Common.Extensions
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Một lỗi không mong đợi đã xảy ra: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var statusCode = (int)HttpStatusCode.InternalServerError;
            var message = "Đã có lỗi hệ thống xảy ra. Vui lòng thử lại sau.";

            switch (exception)
            {
                case UnauthorizedException authEx:
                    statusCode = authEx.StatusCode;
                    message = authEx.Message;
                    break;
                case ForbiddenException forbEx:
                    statusCode = forbEx.StatusCode;
                    message = forbEx.Message;
                    break;
                case BadRequestException badRequestEx:
                    statusCode = badRequestEx.StatusCode;
                    message = badRequestEx.Message;
                    break;
                case ConflictException conflictEx:
                    statusCode = conflictEx.StatusCode;
                    message = conflictEx.Message;
                    break;
                case NotFoundException notFoundException:
                    statusCode = notFoundException.StatusCode;
                    message = notFoundException.Message;
                    break;
            }

            context.Response.StatusCode = statusCode;

            var response = new
            {
                isSuccess = false,
                statusCode = statusCode,
                error = message,
                detail = _env.IsDevelopment() ? exception.ToString() : null
            };

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(response, options);

            await context.Response.WriteAsync(json);
        }
    }
}
