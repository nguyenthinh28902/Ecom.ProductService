using Ecom.ProductService.Core.Models;
using Ecom.ProductService.Core.Models.Dtos.Brand;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Application.Interface.CMS
{
     public interface IBrandManagerService
    {
        Task<Result<BrandResponse>> CreateBrandAsync(CreateBrandDto request);
    }
}
