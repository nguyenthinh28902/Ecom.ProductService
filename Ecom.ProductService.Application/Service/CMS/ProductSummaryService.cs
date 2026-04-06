using Ecom.ProductService.Application.Interface.Auth;
using Ecom.ProductService.Application.Interface.CMS;
using Ecom.ProductService.Core.Abstractions.Persistence;
using Ecom.ProductService.Core.Abstractions.Persistence.ReadOnly;
using Ecom.ProductService.Core.Entities;
using Ecom.ProductService.Core.Enums;
using Ecom.ProductService.Core.Models;
using Ecom.ProductService.Core.Models.Auth;
using Ecom.ProductService.Core.Models.Dtos.ProductSummaryService;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Application.Service.CMS
{
    public class ProductSummaryService : IProductSummaryService
    {
        private readonly IProductSummaryReadOnlyRepository _summaryRepo;
        private readonly IBaseService _baseService;

        public ProductSummaryService(IProductSummaryReadOnlyRepository summaryRepo, IBaseService baseService)
        {
            _summaryRepo = summaryRepo;
            _baseService = baseService;
        }

        public async Task<Result<DashboardProductDto>> GetDashboardStats()
        {
            _baseService.EnsurePermission(ProductPermission.ProductRead);
            var dto = new DashboardProductDto();
            var today = DateTime.UtcNow.Date;

            // Gọi Repo để lấy các chỉ số từ Database Slave
            var brandCount = await _summaryRepo.CountActiveBrandsAsync();
            var catCount = await _summaryRepo.CountActiveCategoriesAsync();
            var totalActive = await _summaryRepo.CountActiveProductsAsync();
            var pendingCount = await _summaryRepo.CountPendingProductsAsync(today);

            dto.SummaryMetrics.Add(new SummaryMetrics { Title = "Thương hiệu", value = brandCount });
            dto.SummaryMetrics.Add(new SummaryMetrics { Title = "Ngành hàng", value = catCount });
            dto.SummaryMetrics.Add(new SummaryMetrics { Title = "Sản phẩm đang kinh doanh", value = totalActive, Group = "Group.Products", TitleGroup = "Sản phẩm" });
            dto.SummaryMetrics.Add(new SummaryMetrics { Title = "Sản phẩm đang chờ mở bán", value = pendingCount, Group = "Group.Products", TitleGroup = "Sản phẩm" });
            dto.SummaryMetrics.Add(new SummaryMetrics { Title = "Sản phẩm đang mở bán", value = totalActive - pendingCount, Group = "Group.Products", TitleGroup = "Sản phẩm" });

            return Result<DashboardProductDto>.Success(dto);
        }
    }
}
