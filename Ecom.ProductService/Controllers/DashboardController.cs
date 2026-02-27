using Ecom.ProductService.Application.Interface.CMS;
using Ecom.ProductService.Core.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecom.ProductService.Controllers
{
    [Route("api/nganh-hang/quan-ly/thong-ke")]
    [ApiController]
    [Authorize(PolicyNames.ProductRead)]
    public class DashboardController : ControllerBase
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly IProductSummaryService _productSummaryService;
        public DashboardController(ILogger<DashboardController> logger,
            IProductSummaryService productSummaryService)
        {
            _logger = logger;
            _productSummaryService = productSummaryService;
        }

        [HttpGet("noi-dung")]
        public async Task<IActionResult> GetDashboardContent()
        {
            var dashboardContent = await _productSummaryService.GetDashboardStats();
            return Ok(dashboardContent);
        }
    }
}
