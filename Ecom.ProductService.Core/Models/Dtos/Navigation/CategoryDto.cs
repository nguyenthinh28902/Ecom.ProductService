using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Core.Models.Dtos.Navigation
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string NameAscii { get; set; } = null!;
        public string? IconPath { get; set; }
        public List<CategoryDto> SubCategories { get; set; } = new(); // Cho menu đa cấp
    }
}
