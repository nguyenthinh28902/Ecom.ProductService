using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Core.Abstractions.Persistence.ReadOnly
{
    public interface IProductSummaryReadOnlyRepository
    {
        Task<int> CountActiveBrandsAsync();
        Task<int> CountActiveCategoriesAsync();
        Task<int> CountActiveProductsAsync();
        Task<int> CountPendingProductsAsync(DateTime today);
    }
}
