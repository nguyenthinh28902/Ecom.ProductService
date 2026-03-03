using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Core.Models.Dtos.Navigation
{
    public class ProductFilterMenuViewModel
    {
        public List<BrandDto> Brands { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new();
    }
}
