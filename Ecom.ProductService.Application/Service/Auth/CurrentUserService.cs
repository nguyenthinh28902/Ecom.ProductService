using Ecom.ProductService.Application.Interface.Auth;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Application.Service.Auth
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpContext? HttpContext => _httpContextAccessor.HttpContext;

        // Ưu tiên đọc từ Header do Gateway truyền xuống
        public int UserId => int.TryParse(HttpContext?.Request.Headers["X-User-Id"], out var userId) ? userId : 0;

        public string? Email => HttpContext?.Request.Headers["X-User-Email"].ToString();

        // Lấy danh sách Roles từ Header (Gateway gửi dạng: "Admin,Manager")
        public List<string> Roles =>
            HttpContext?.Request.Headers["X-User-Roles"].ToString()
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(r => r.Trim())
                .ToList() ?? new List<string>();

        // WorkplaceId cũng lấy từ Header
        public int WorkplaceId => int.TryParse(HttpContext?.Request.Headers["X-User-WorkplaceId"], out var wid) ? wid : 0;

        // Role đơn lẻ (nếu ný cần lấy cái đầu tiên)
        public string? Role => Roles.FirstOrDefault();
        public List<string> Scopes =>
           HttpContext?.Request.Headers["X-User-Scopes"].ToString()
               .Split(',', StringSplitOptions.RemoveEmptyEntries)
               .Select(r => r.Trim())
               .ToList() ?? new List<string>();
        // Lấy IP Address
        public string? IpAddress =>
            HttpContext?.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? // Lấy IP thật nếu qua Gateway/Proxy
            HttpContext?.Connection.RemoteIpAddress?.ToString(); // Lấy IP trực tiếp nếu không qua Proxy

        // Lấy User Agent (Thông tin trình duyệt/thiết bị)
        public string? UserAgent => HttpContext?.Request.Headers["User-Agent"].ToString();
    }
}
