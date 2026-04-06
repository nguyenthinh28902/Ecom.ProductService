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
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Application.Service.Web
{
    public class NavigationService : INavigationService
    {
        private readonly ILogger<NavigationService> _logger;
        private readonly IBrandReadOnlyRepository _brandRepo;
        private readonly ICategoryReadOnlyRepository _categoryRepo;
        private readonly ICacheService _cacheService;

        public NavigationService(
            IBrandReadOnlyRepository brandRepo,
            ICategoryReadOnlyRepository categoryRepo,
            ICacheService cacheService,
            ILogger<NavigationService> logger) 
        {
            _brandRepo = brandRepo;
            _categoryRepo = categoryRepo;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<Result<ProductFilterMenuViewModel>> GetNavigationHomeAsync()
        {
            var cacheKey = "NavigationHome";
            var cachedData = await _cacheService.GetAsync<ProductFilterMenuViewModel>(cacheKey);
            if (cachedData != null) return Result<ProductFilterMenuViewModel>.Success(cachedData, "Lấy thông tin thành công.");

            //  Gọi Repo chuyên biệt cho luồng Đọc
            var brands = await _brandRepo.GetNavigationBrandsAsync();
            var categories = await _categoryRepo.GetNavigationCategoriesAsync();

            var result = new ProductFilterMenuViewModel { Brands = brands, Categories = categories };
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(60));

            return Result<ProductFilterMenuViewModel>.Success(result, "Lấy thông tin thành công.");
        }
    }
}
