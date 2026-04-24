using Ecom.ProductService.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Application.Interface.CMS
{
    public interface IProductManagerService
    {
        Task<Result<bool>> ProductDelete(int productId);
    }
}
