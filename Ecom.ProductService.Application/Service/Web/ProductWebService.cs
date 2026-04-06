using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecom.ProductService.Application.Interface;
using Ecom.ProductService.Application.Interface.Web;
using Ecom.ProductService.Core.Abstractions.Persistence;
using Ecom.ProductService.Core.Abstractions.Persistence.ReadOnly;
using Ecom.ProductService.Core.Entities;
using Ecom.ProductService.Core.Enums;
using Ecom.ProductService.Core.Models;
using Ecom.ProductService.Core.Models.Dtos.Navigation;
using Ecom.ProductService.Core.Models.Dtos.ProductWeb;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Application.Service.Web
{
    public class ProductWebService : IProductWebService
    {
        private readonly ILogger<ProductWebService> _logger;
        private readonly IProductReadOnlyRepository _productRepo;
        private readonly ICacheService _cacheService;
        public ProductWebService(IProductReadOnlyRepository productRepo, ICacheService cacheService
            , ILogger<ProductWebService> logger)
        {
            _productRepo = productRepo;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<Result<HomeProductDisplayDto>> GetProductHome()
        {
            var cacheKey = "GetProductHome";
            var cachedData = await _cacheService.GetAsync<HomeProductDisplayDto>(cacheKey);
            if (cachedData != null) return Result<HomeProductDisplayDto>.Success(cachedData);

            var thirtyDaysAgo = DateTime.UtcNow.Date.AddDays(-30);
            var allRelevantProducts = await _productRepo.GetRawHomeProductsAsync(thirtyDaysAgo);

            var homeProducts = new HomeProductDisplayDto
            {
                NewArrivals = allRelevantProducts.Where(x => x.PublishDate > thirtyDaysAgo).Take(20).ToList(),
                BestDeals = allRelevantProducts.OrderByDescending(p => p.Price).Take(5).ToList()
            };

            await _cacheService.SetAsync(cacheKey, homeProducts, TimeSpan.FromMinutes(60));
            return Result<HomeProductDisplayDto>.Success(homeProducts);
        }



        /// <summary>
        /// Get danh sách sản phẩm theo yêu cầu
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<Result<ProductListResponseDto>> GetProducts(ProductQueryParameters query)
        {
            var (items, total) = await _productRepo.GetPagedProductsAsync(query);

            var response = new ProductListResponseDto
            {
                Products = items,
                TotalItems = total,
                CurrentPage = query.Page,
                TotalPages = (int)Math.Ceiling(total / (double)query.PageSize)
            };
            return Result<ProductListResponseDto>.Success(response);
        }

        /// <summary>
        /// lấy chi tiết sản phẩm
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public async Task<Result<ProductDetailDto>> GetProductDetail(string slug, string? version)
        {
            _logger.LogInformation($"{nameof(GetProductDetail)} start: {slug}");

            var isDefault = string.IsNullOrEmpty(version);
            version ??= string.Empty;

            // 1. Lấy thông tin cơ bản và Variant qua Repo
            var dto = await _productRepo.GetProductDetailWithVariantsAsync(slug, version, isDefault);

            if (dto == null) return Result<ProductDetailDto>.Failure("Sản phẩm không tồn tại.");

            // 2. Lấy Specifications (Đã được Repo Group sẵn)
            dto.Specifications = await _productRepo.GetProductAttributesAsync(dto.Id);

            // 3. Lấy Breadcrumbs
            dto.Breadcrumbs = await _productRepo.GetBreadcrumbsAsync(dto.Id);

            // 4. Logic xử lý RAM: Đánh dấu sản phẩm hiện tại trong Group
            if (dto.GroupProducts != null)
            {
                foreach (var p in dto.GroupProducts)
                    p.IsCurrent = p.NameAscii == slug;
            }

            return Result<ProductDetailDto>.Success(dto, "Thành công.");
        }
      
    }
}
