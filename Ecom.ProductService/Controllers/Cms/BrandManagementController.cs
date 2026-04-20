using Ecom.ProductService.Application.Interface.CMS;
using Ecom.ProductService.Core.Models.Auth;
using Ecom.ProductService.Core.Models.Dtos.Brand;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecom.ProductService.Controllers.Cms
{
    [Route("api/nganh-hang/quan-ly/thuong-hieu")]
    [ApiController]
   // [Authorize(PolicyNames.ProductRead)]

    public class BrandManagementController : ControllerBase
    {
        public readonly ILogger<BrandManagementController> _logger;
        private readonly IBrandManagerService _brandManagerService;
        public BrandManagementController(ILogger<BrandManagementController> logger, IBrandManagerService brandManagerService)
        {
            _logger = logger;
            _brandManagerService = brandManagerService;
        }

        [HttpPost("create")]
        //[Authorize(PolicyNames.ProductWrite)]
        public async Task<IActionResult> CreateBrand([FromBody] CreateBrandDto request)
        {
            _logger.LogInformation("Nhận yêu cầu tạo thương hiệu mới: {BrandName}", request.Name);

            // Gọi Service xử lý nghiệp vụ (bao gồm Transaction, Log, Check trùng...)
            var result = await _brandManagerService.CreateBrandAsync(request);

            return Ok(result);
        }
    }
}
