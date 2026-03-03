using Ecom.ProductService.Core.Models;
using Ecom.ProductService.Core.Models.Dtos.Navigation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Application.Interface.Web
{
    public interface INavigationService
    {
       Task<Result<ProductFilterMenuViewModel>> GetNavigationHomeAsync();
    }
}
