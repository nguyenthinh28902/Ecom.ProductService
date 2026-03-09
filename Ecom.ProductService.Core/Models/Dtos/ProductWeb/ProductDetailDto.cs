using Ecom.ProductService.Core.Models.Dtos.Navigation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Core.Models.Dtos.ProductWeb
{
    public class ProductDetailDto
    {
        // --- Thông tin cơ bản ---
        public int Id { get; set; }
        public int VariantId { get; set; }
        public string Name { get; set; } = null!;
        public string BrandName { get; set; } = string.Empty;
        public string NameAscii { get; set; } = null!;
        public string Sku { get; set; } = null!;
        public string? VersionName { get; set; } // Ví dụ: "128GB"
        public string? ShortDescription { get; set; }
        public string? FullDescription { get; set; } // Map từ ProductGroup.Description
        public bool IsActive { get; set; }
        public string StatusString => IsActive ? "Đang kinh doanh" : "Tạm ngưng kinh doanh";  // tạm

        // --- Giá & Kho (Dùng cho bản mặc định) ---
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public int? DiscountPercentage { get; set; }
        public bool IsStockAvailable { get; set; }
        public string CurrencyUnit { get; set; } = "₫";

        // --- Phân nhánh (Breadcrumbs) ---
        // Laptop > Gaming > ASUS
        public List<BreadcrumbDto> Breadcrumbs { get; set; } = new();



        // --- Media (Ảnh Slide) ---
        public List<ProductImageDto> Images { get; set; } = new();

        // --- Biến thể & Nhóm (Chọn Màu/Chọn Model) ---
        public List<ProductVariantDto> Variants { get; set; } = new(); // Chọn Màu
        public List<RelatedProductDto> GroupProducts { get; set; } = new(); // Các model khác trong cùng ProductGroup

        // --- Thông số kỹ thuật (Specs) ---
        public List<AttributeGroupDto> Specifications { get; set; } = new();
    }

    // --- Các Class hỗ trợ ---

    public class BreadcrumbDto
    {
        public string Name { get; set; } = null!;
        public string NameAscii { get; set; } = null!;
    }

    public class ProductImageDto
    {
        public string ImagePath { get; set; } = null!;
        public string? AltText { get; set; }
        public int? VariantId { get; set; } // Dùng để đổi ảnh khi chọn màu
    }

    public class ProductVariantDto
    {
        public int Id { get; set; }
        public string ColorName { get; set; } = null!;
        public string NameAscii { get; set; } = null!;
        public string? ColorCode { get; set; } // Mã HEX: #000000
        public decimal Price { get; set; }
        public string Sku { get; set; } = null!;
        public bool IsDefault { get; set; }
    }

    public class RelatedProductDto
    {
        public string Name { get; set; } = null!;
        public string NameAscii { get; set; } = null!;
        public string? VersionName { get; set; }
        public bool IsCurrent { get; set; } // Đánh dấu model đang xem
    }

    public class AttributeGroupDto
    {
        public string GroupName { get; set; } = null!; // Ví dụ: "Màn hình"
        public List<AttributeValueDto> Attributes { get; set; } = new();
    }

    public class AttributeValueDto
    {
        public string Key { get; set; } = null!;   // Ví dụ: "Kích thước"
        public string Value { get; set; } = null!; // Ví dụ: "6.7 inch"
    }
}
