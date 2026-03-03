using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecom.ProductService.Application.Interface;
using Ecom.ProductService.Application.Interface.Web;
using Ecom.ProductService.Core.Abstractions.Persistence;
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
        private readonly ICacheService _cacheService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public NavigationService(ILogger<NavigationService> logger, ICacheService cacheService, IUnitOfWork unitOfWork
            ,IMapper mapper) { 
            _logger = logger;
            _cacheService = cacheService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<Result<ProductFilterMenuViewModel>> GetNavigationHomeAsync()
        {
            var cacheKey = "NavigationHome";
            var cachedData = await _cacheService.GetAsync<ProductFilterMenuViewModel>(cacheKey);
            if (cachedData != null) return Result<ProductFilterMenuViewModel>.Success(cachedData, "Dữ liệu danh mục");
            var brands = _unitOfWork.Repository<Brand>().GetAll(x => x.IsDeleted != true && x.Status == (byte)EntityStatus.Active)
                .ProjectTo<BrandDto>(_mapper.ConfigurationProvider).ToList();
            var categories = _unitOfWork.Repository<Category>().GetAll(x => x.IsDeleted != true && x.Status == (byte)EntityStatus.Active)
                .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider).ToList();

            var result = new ProductFilterMenuViewModel();
            result.Brands = brands;
            result.Categories = categories;
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(60));
            return Result<ProductFilterMenuViewModel>.Success(result, "Dữ liệu danh mục");

        }
    }
}
