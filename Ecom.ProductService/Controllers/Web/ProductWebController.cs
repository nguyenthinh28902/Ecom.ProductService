using Ecom.ProductService.Application.Interface.Web;
using Ecom.ProductService.Core.Models;
using Ecom.ProductService.Core.Models.Auth;
using Ecom.ProductService.Core.Models.Dtos.ProductWeb;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecom.ProductService.Controllers.Web
{
    [Route("api/nganh-hang/san-pham")]
    [ApiController]
    // hệ thống gọi vẫn có token vì khi gọi qua gateway => gateway sẽ thêm token vào header, nhưng token này sẽ chỉ có quyền đọc web nên sẽ không gọi được các API dành riêng cho internal
   // [Authorize(PolicyNames.ProductReadWeb)]
    public class ProductWebController : ControllerBase
    {
        private readonly ILogger<ProductWebController> _logger;
        private readonly IProductWebService _productWebService; 
        public ProductWebController(ILogger<ProductWebController> logger
            , IProductWebService productWebService)
        {
            _logger = logger;
            _productWebService = productWebService;
        }

        /// <summary>
        /// Lấy danh sách 5 sản phẩm có giá bán cao nhất hệ thống.
        /// </summary>
        /// <returns>Danh sách ProductCardDto chứa thông tin rút gọn.</returns>
        /// <response code="200">Trả về danh sách sản phẩm thành công.</response>
        /// <response code="404">Không tìm thấy sản phẩm nào phù hợp.</response>
        [HttpGet("san-pham-trang-chu")]
        [ProducesResponseType(typeof(IEnumerable<Result<HomeProductDisplayDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductHome()
        {
            var products = await _productWebService.GetProductHome();
            return Ok(products);
        }

        /// <summary>
        /// lấy sản phảm trang danh sách sản phẩm
        /// </summary>
        /// <param name="productQueryParameters"></param>
        /// <returns></returns>
        [HttpPost("danh-sach-san-pham")]
        [ProducesResponseType(typeof(IEnumerable<Result<ProductListResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
       
        public async Task<IActionResult> GetProducts([FromBody]ProductQueryParameters productQueryParameters)
        {
            var result = await _productWebService.GetProducts(productQueryParameters);
            return Ok(result);
        }

        /// <summary>
        /// lấy sản phảm trang danh sách sản phẩm
        /// </summary>
        /// <param name="productQueryParameters"></param>
        /// <returns></returns>
        [HttpGet("{Slug}")]
        [ProducesResponseType(typeof(IEnumerable<Result<ProductDetailDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        
        public async Task<IActionResult> GetProductDetail(string Slug, [FromQuery(Name = "phienban")] string? version = null)
        {
            var result = await _productWebService.GetProductDetail(Slug, version);
            return Ok(result);
        }
    }
}
