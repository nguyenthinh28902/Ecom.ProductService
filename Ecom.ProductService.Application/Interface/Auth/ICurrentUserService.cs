using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Application.Interface.Auth
{
    public interface ICurrentUserService
    {
        int UserId { get; }
        string? Email { get; }
        string? Role { get; }
        int WorkplaceId { get; }
        List<string> Scopes { get; }
        List<string> Roles { get; }
    }
}
