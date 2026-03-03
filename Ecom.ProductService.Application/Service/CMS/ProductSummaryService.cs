using Ecom.ProductService.Application.Interface.Auth;
using Ecom.ProductService.Application.Interface.CMS;
using Ecom.ProductService.Core.Abstractions.Persistence;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductSummaryService> _logger;
        private readonly IBaseService _baseService;
        public ProductSummaryService(ILogger<ProductSummaryService> logger, IUnitOfWork unitOfWork
            , IBaseService baseService) 
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _baseService = baseService;
        }
        
        public async Task<Result<DashboardProductDto>> GetDashboardStats()
        {
            _baseService.EnsurePermission(ProductPermission.ProductRead);
            var dashboardProductDto = new DashboardProductDto();
            var i = await _unitOfWork.Repository<Brand>().CountAsync(x => x.IsDeleted != true);
            dashboardProductDto.SummaryMetrics.Add(new SummaryMetrics() { Title = "Thương hiệu", value = await _unitOfWork.Repository<Brand>().CountAsync(x => x.IsDeleted != true) });
            dashboardProductDto.SummaryMetrics.Add(new SummaryMetrics() { Title = "Ngành hàng", value = await _unitOfWork.Repository<Category>().CountAsync(x => x.IsDeleted != true) });
           var totalProducts = await _unitOfWork.Repository<Product>().CountAsync(x => x.IsDeleted != true && x.Status == (byte)EntityStatus.Active);
            dashboardProductDto.SummaryMetrics.Add(new SummaryMetrics() { 
                Title = "Sản phẩm đang kinh doanh", 
                value = totalProducts,
                Group = "Group.Products",
                TitleGroup = "Sản phẩm"
            });
            var pendingProductsCount = await _unitOfWork.Repository<Product>().CountAsync(x => x.IsDeleted != true && x.Status == (byte)EntityStatus.Active && x.PublishDate <= DateTime.UtcNow.Date);
            dashboardProductDto.SummaryMetrics.Add(new SummaryMetrics()
            {
                Title = "Sản phẩm đang chờ mở bán",
                value = pendingProductsCount,
                Group = "Group.Products",
                TitleGroup = "Sản phẩm"
            });
            dashboardProductDto.SummaryMetrics.Add(new SummaryMetrics()
            {
                Title = "Sản phẩm đang mở bán",
                value = totalProducts - pendingProductsCount,
                Group = "Group.Products",
                TitleGroup = "Sản phẩm"
            });
            
            return Result<DashboardProductDto>.Success(dashboardProductDto, "Thành công.");
        }


    }
}
