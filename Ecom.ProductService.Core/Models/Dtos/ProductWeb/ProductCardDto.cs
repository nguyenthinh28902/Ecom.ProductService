using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Core.Models.Dtos.ProductWeb
{
    public class ProductCardDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string NameAscii { get; set; } = null!;
        public string? MainImage { get; set; }

        // Giá gốc để hiển thị gạch ngang khi có giảm giá
        public decimal OriginalPrice { get; set; }
        public decimal Price { get; set; }
        public string? CurrencyUnit { get; set; }

        // Tên thương hiệu để hiển thị trên thẻ sản phẩm
        public string? BrandName { get; set; }

        // Thuộc tính bổ sung để gắn nhãn (New, Sale, v.v.)
        public bool IsNewArrival => PublishDate.HasValue && PublishDate.Value > DateTime.Now.AddDays(-30);
        public DateTime? PublishDate { get; set; }

        // Tính toán % giảm giá trực tiếp để hiển thị Badge
        public int DiscountPercentage => OriginalPrice > Price
            ? (int)Math.Round((OriginalPrice - Price) / OriginalPrice * 100)
            : 0;
    }
}
