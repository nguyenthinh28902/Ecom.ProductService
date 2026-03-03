using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Core.Models.Dtos.ProductWeb
{
    public class ProductQueryParameters
    {
        // Slug của Category (ví dụ: laptop, dtdd)
        public string? CategorySlug { get; set; }

        // Slug của Brand (ví dụ: asus, apple)
        public string? BrandSlug { get; set; }

        // Phân trang
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Sắp xếp: "price_asc", "price_desc", "newest"
        public string? SortBy { get; set; }

        // Tìm kiếm từ khóa (nếu cần)
        public string? SearchTerm { get; set; }
    }
}
