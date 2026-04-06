using Ecom.ProductService.Core.Entities;
using Ecom.ProductService.Core.Models.Dtos.ProductWeb;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Core.Abstractions.Persistence.ReadOnly
{
    public interface IProductReadOnlyRepository
    {
        // Lấy data thô cho trang chủ
        Task<List<ProductCardDto>> GetRawHomeProductsAsync(DateTime thirtyDaysAgo);

        // Lấy danh sách sản phẩm có phân trang và lọc
        Task<(List<ProductCardDto> Items, int Total)> GetPagedProductsAsync(ProductQueryParameters query);

        // Lấy chi tiết sản phẩm và các thuộc tính đi kèm
        Task<ProductDetailDto?> GetProductDetailWithVariantsAsync(string slug, string version, bool isDefault);

        // Lấy danh sách thuộc tính (Specifications)
        Task<List<AttributeGroupDto>> GetProductAttributesAsync(int productId);

        // Lấy Category chính để làm Breadcrumb
        Task<Category?> GetMainCategoryByProductIdAsync(int productId);
        //đọc dữ liệu để làm breadcrumb
        Task<List<BreadcrumbDto>> GetBreadcrumbsAsync(int productId);
    }
}
