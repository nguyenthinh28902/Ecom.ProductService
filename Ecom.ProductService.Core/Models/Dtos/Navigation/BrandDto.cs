using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Core.Models.Dtos.Navigation
{
    public class BrandDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string NameAscii { get; set; } = null!;
        public string? LogoUrl { get; set; }
    }
}
