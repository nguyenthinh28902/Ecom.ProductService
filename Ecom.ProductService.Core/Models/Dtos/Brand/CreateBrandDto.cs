using Ecom.ProductService.Core.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ecom.ProductService.Core.Models.Dtos.Brand
{
    public class CreateBrandDto
    {
        [Required(ErrorMessage = "Tên thương hiệu không được để trống")]
        [StringLength(255)]
        public string Name { get; set; } = null!;

        public string NameAscii { get; set; } = null!;

        [StringLength(100)]
        public string? Origin { get; set; }

        public EntityStatus Status { get; set; }

        /// <summary>
        /// File logo tải lên từ Client
        /// </summary>
        public IFormFile? LogoFile { get; set; }
    }
}
