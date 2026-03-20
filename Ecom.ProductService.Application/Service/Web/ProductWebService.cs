using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecom.ProductService.Application.Interface;
using Ecom.ProductService.Application.Interface.Web;
using Ecom.ProductService.Core.Abstractions.Persistence;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        public ProductWebService(ILogger<ProductWebService> logger
            , IUnitOfWork unitOfWork
            ,IMapper mapper,
            ICacheService cacheService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<Result<HomeProductDisplayDto>> GetProductHome()
        {
            _logger.LogInformation($"{nameof(GetProductHome)} start");
            var cacheKey = "GetProductHome";

            // 1. Thử lấy từ Cache (Fail-safe: Nếu Redis lỗi, hàm này trả về null nhờ try-catch bên trong)
            var cachedData = await _cacheService.GetAsync<HomeProductDisplayDto>(cacheKey);
            if (cachedData != null)
            {
                return Result<HomeProductDisplayDto>.Success(cachedData, "Danh sách sản phẩm trang chủ");
            }

            var today = DateTime.UtcNow.Date;
            var thirtyDaysAgo = today.AddDays(-30);

            // 2. Lấy tập dữ liệu gộp (Union) cả 2 điều kiện trong 1 lần gọi DB
            // Sử dụng .AsNoTracking() để EF Core không tốn tài nguyên theo dõi object
            var allRelevantProducts = await _unitOfWork.Repository<Product>()
                .GetAll(x => x.Status == (byte)EntityStatus.Active && x.PublishDate <= today)
                .Where(x => x.PublishDate > thirtyDaysAgo && x.IsDeleted != true) // Lấy các SP mới HOẶC có giá (để lọc BestDeals)
                .OrderByDescending(x => x.PublishDate)
                .Take(100) // Giới hạn lấy 100 bản ghi thô để phân loại trên RAM
                .AsNoTracking()
                .ProjectTo<ProductCardDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            // 3. Phân loại trên RAM (LINQ to Object - tốc độ tính bằng micro-giây)
            var homeProducts = new HomeProductDisplayDto
            {
                // Lấy sản phẩm mới trong vòng 30 ngày qua
                NewArrivals = allRelevantProducts
                    .Where(x => x.PublishDate > thirtyDaysAgo)
                    .Take(20)
                    .ToList(),

                // Lấy 5 sản phẩm giá cao nhất (giả định là BestDeals) từ tập data đã lấy
                BestDeals = allRelevantProducts
                    .OrderByDescending(p => p.Price)
                    .Take(5)
                    .ToList()
            };

            _logger.LogInformation($"{nameof(GetProductHome)} end");

            // 4. Lưu Cache (Fire-and-forget hoặc await tùy nhu cầu, nên để 60p như cũ)
            await _cacheService.SetAsync(cacheKey, homeProducts, TimeSpan.FromMinutes(60));

            return Result<HomeProductDisplayDto>.Success(homeProducts, "Danh sách sản phẩm trang chủ");
        }



        /// <summary>
        /// Get danh sách sản phẩm theo yêu cầu
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<Result<ProductListResponseDto>> GetProducts(ProductQueryParameters query)
        {
            _logger.LogInformation($"{nameof(GetProducts)} start: Category={query.CategorySlug}");

            // 1. Khởi tạo Queryable từ Repository Product
            var queryable = _unitOfWork.Repository<Product>()
                .GetAll(x => x.Status == (byte)EntityStatus.Active && x.PublishDate <= DateTime.UtcNow && x.IsDeleted != true);

            
            if (!string.IsNullOrEmpty(query.CategorySlug))
            {
                // Kiểm tra xem sản phẩm có bất kỳ mapping nào khớp với Slug của Category không
                queryable = queryable.Where(x => x.ProductCategoryMappings
                    .Any(m => m.Category.NameAscii == query.CategorySlug));
                _logger.LogInformation($"{nameof(GetProducts)} filter by CategorySlug: {queryable.Count()}");
            }

            
            if (!string.IsNullOrEmpty(query.BrandSlug))
            {
                var cleanBrandSlug = query.BrandSlug;
                queryable = queryable.Where(x => x.Brand.NameAscii == cleanBrandSlug);
                _logger.LogInformation($"{nameof(GetProducts)} filter by CategorySlug: {queryable.Count()}");
            }
            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                // Làm sạch từ khóa: xóa khoảng trắng thừa ở đầu/cuối và đưa về chữ thường
                var searchTerm = query.SearchTerm.Trim().ToLower();

                queryable = queryable.Where(x =>
                    x.Name.ToLower().Contains(searchTerm) ||
                    x.Sku.ToLower().Contains(searchTerm)
                );
            }
            // 4. Phân trang và thực thi
            int totalItems = await queryable.CountAsync();
            var items = await queryable
                .OrderByDescending(x => x.CreatedAt)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ProjectTo<ProductCardDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var response = new ProductListResponseDto
            {
                Products = items,
                TotalItems = totalItems,
                CurrentPage = query.Page,
                TotalPages = (int)Math.Ceiling(totalItems / (double)query.PageSize)
            };

            _logger.LogInformation($"{nameof(GetProducts)} end: Tìm thấy {totalItems} sản phẩm");
            return Result<ProductListResponseDto>.Success(response, "Dữ liệu load thành công");
        }

        /// <summary>
        /// lấy chi tiết sản phẩm
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public async Task<Result<ProductDetailDto>> GetProductDetail(string slug, string? version)
        {
            _logger.LogInformation($"{nameof(GetProductDetail)} start: {slug}");

            int? selectedVariantId = null;
            var isDefault = false;
            if (string.IsNullOrEmpty(version)) { isDefault = true; version = string.Empty; }
            var productQuery = _unitOfWork.Repository<Product>()
                .GetAll(x => x.NameAscii == slug && x.IsDeleted != true)
                .Include(x => x.ProductVariants.Where(y => y.NameAscii.ToLower() == version.ToLower() || y.IsDefault == isDefault && y.IsActive == true));
             

            var dto = await productQuery
                .ProjectTo<ProductDetailDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (dto == null) return Result<ProductDetailDto>.Failure("Sản phẩm không tồn tại.");
            dto.VariantId = selectedVariantId ?? 0;
            // 2. Xác định Variant mục tiêu dựa trên version (Xử lý trên RAM)

            // 4. Lấy các thông tin phụ trợ tuần tự (Tránh lỗi DbContext)
            var attributes = await _unitOfWork.Repository<ProductAttributeValue>()
                .GetAll(x => x.ProductId == dto.Id)
                .Select(a => new {
                    GroupName = a.Attribute.Group.Name,
                    Key = a.Attribute.Name,
                    Value = a.Value
                }).ToListAsync();

            dto.Specifications = attributes
                .GroupBy(a => a.GroupName)
                .Select(g => new AttributeGroupDto
                {
                    GroupName = g.Key,
                    Attributes = g.Select(x => new AttributeValueDto { Key = x.Key, Value = x.Value }).ToList()
                }).ToList();

            dto.Breadcrumbs = await GetBreadcrumbsByProductId(dto.Id);

            // 5. IsCurrent cho Group
            foreach (var p in dto.GroupProducts) p.IsCurrent = p.NameAscii == slug;

            return Result<ProductDetailDto>.Success(dto, "Thành công.");
        }
        // Logic lấy Breadcrumb tối ưu
        private async Task<List<BreadcrumbDto>> GetBreadcrumbsByProductId(int productId)
        {
            var breadcrumbs = new List<BreadcrumbDto>();

            // Lấy category chính và cấp cha của nó ngay lập tức (Eager loading) để giảm số lần query trong while
            var mainCategory = await _unitOfWork.Repository<ProductCategoryMapping>()
                .GetAll(m => m.ProductId == productId && m.IsMain == true)
                .Select(m => m.Category)
                .FirstOrDefaultAsync();

            var currentCat = mainCategory;
            while (currentCat != null)
            {
                breadcrumbs.Insert(0, new BreadcrumbDto
                {
                    Name = currentCat.Name,
                    NameAscii = currentCat.NameAscii
                });

                if (!currentCat.ParentId.HasValue || currentCat.ParentId == 0) break;

                // Truy vấn cấp cha tiếp theo
                // Dùng GetAll thay vì FindAsync để đồng bộ với cách dùng Repository của ông
                currentCat = await _unitOfWork.Repository<Category>()
                    .GetAll(c => c.Id == currentCat.ParentId)
                    .FirstOrDefaultAsync();
            }

            return breadcrumbs;
        }

        
    }
}
