using Ecom.ProductService.Core.Models.Dtos.Navigation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Core.Abstractions.Persistence.ReadOnly
{
    public interface IBrandReadOnlyRepository
    {
        Task<List<BrandDto>> GetNavigationBrandsAsync();
    }
}
