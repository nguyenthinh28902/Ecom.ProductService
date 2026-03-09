using Ecom.ProductService.Core.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecom.ProductService.Controllers.Cms
{
    [Route("api/nganh-hang/quan-ly/san-pham")]
    [ApiController]
    [Authorize(PolicyNames.ProductRead)]
    public class ProductManagementController : ControllerBase
    {
    }
}
