using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Core.Models.Dtos.ProductWeb
{
    public class ProductListResponseDto
    {
        // Danh sách sản phẩm dùng chung Card cũ của ông
        public List<ProductCardDto> Products { get; set; } = new List<ProductCardDto>();

        // Thông tin phân trang
        public int TotalItems { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        // Thông tin bổ trợ để hiển thị UI
        public string? CategoryName { get; set; }
        public string? BrandName { get; set; }
    }
}
