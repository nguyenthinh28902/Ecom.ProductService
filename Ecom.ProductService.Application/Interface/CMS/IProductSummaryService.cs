using Ecom.ProductService.Core.Models;
using Ecom.ProductService.Core.Models.Dtos.ProductSummaryService;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Application.Interface.CMS
{
    public interface IProductSummaryService
    {
        public Task<Result<DashboardProductDto>> GetDashboardStats();
    }
}
