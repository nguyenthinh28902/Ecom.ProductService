using Microsoft.AspNetCore.Authorization;

namespace Ecom.ProductService.Common.Requirement
{
    public class InternalOrPermissionRequirement : IAuthorizationRequirement
    {
        public string RequiredPermission { get; }
        public InternalOrPermissionRequirement(string permission)
        {
            RequiredPermission = permission;
        }
    }
}
