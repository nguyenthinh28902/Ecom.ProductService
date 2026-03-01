using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Core.Models.Dtos.ProductWeb
{
    public class HomeProductDisplayDto
    {
        public List<ProductCardDto> NewArrivals { get; set; } = new();

        // Danh sách sản phẩm đang giảm giá sâu (Flash Sale)
        public List<ProductCardDto> BestDeals { get; set; } = new();
    }
}
