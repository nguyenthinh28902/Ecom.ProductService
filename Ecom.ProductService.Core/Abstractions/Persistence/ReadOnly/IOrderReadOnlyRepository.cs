using Ecom.ProductService.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Core.Abstractions.Persistence.ReadOnly
{
    public interface IOrderReadOnlyRepository
    {
        // Lấy thông tin sản phẩm và variant theo danh sách ID
        Task<List<Product>> GetProductsWithVariantsAsync(List<int> productIds, List<int> variantIds);
        // Lấy thông tin kèm ảnh để checkout
        Task<List<Product>> GetProductsForCheckoutAsync(List<int> productIds, List<int> variantIds);
    }
}
