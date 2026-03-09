using Ecom.ProductService.Core.Models;
using Ecom.ProductService.Core.Models.Dtos.ProductWeb;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Application.Interface.Web
{
    public interface IProductWebService
    {
      Task<Result<HomeProductDisplayDto>> GetProductHome();
      Task<Result<ProductListResponseDto>> GetProducts(ProductQueryParameters query);
      Task<Result<ProductDetailDto>> GetProductDetail(string slug, string? version);
     
    }
}
