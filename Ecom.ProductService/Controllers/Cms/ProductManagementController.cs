using Ecom.ProductService.Application.Interface.CMS;
using Ecom.ProductService.Application.Service.CMS;
using Ecom.ProductService.Core.Models.Auth;
using Ecom.ProductService.Core.Models.Dtos.Brand;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecom.ProductService.Controllers.Cms
{
    [Route("api/nganh-hang/quan-ly/san-pham")]
    [ApiController]
    //[Authorize(PolicyNames.ProductRead)]
    public class ProductManagementController : ControllerBase
    {
        private readonly ILogger<ProductManagementController> _logger;
        private readonly IProductManagerService _productManagerService;
        public ProductManagementController(ILogger<ProductManagementController> logger, IProductManagerService productManagerService)
        {   
            _logger = logger;
            _productManagerService = productManagerService;
        }

        [HttpDelete("xoa-san-pham/{id:int}")] // Dùng Route Parameter thay vì Body cho đúng chuẩn DELETE
        //[Authorize(PolicyNames.ProductWrite)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            _logger.LogInformation("Nhận yêu cầu xóa sản phẩm với ID: {ProductId}", id);

            // Gọi Service xử lý nghiệp vụ (bao gồm Transaction, Log, Check trùng...)
            var result = await _productManagerService.ProductDelete(id);

            return Ok(result);
        }
    }
}
