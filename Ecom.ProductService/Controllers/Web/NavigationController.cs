using Ecom.ProductService.Application.Interface.Web;
using Ecom.ProductService.Core.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecom.ProductService.Controllers.Web
{
    [Route("api/nganh-hang/danh-muc")]
    [ApiController]
    [Authorize(PolicyNames.ProductReadWeb)]
    public class NavigationController : ControllerBase
    {
        private readonly ILogger<NavigationController> _logger;
        private readonly INavigationService _navigationService;
        public NavigationController(ILogger<NavigationController> logger, INavigationService navigationService)
        {
            _logger = logger;
            _navigationService = navigationService;
        }

        [HttpGet("danh-muc-trang-chu")]
        public async Task<IActionResult> GetNavigation()
        {           
            var result = await _navigationService.GetNavigationHomeAsync();
            return Ok(result);

        }
    }
}
