using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecom.ProductService.Core.Abstractions.Persistence;
using Ecom.ProductService.Core.Abstractions.Persistence.ReadOnly;
using Ecom.ProductService.Core.Entities;
using Ecom.ProductService.Core.Enums;
using Ecom.ProductService.Core.Models.Dtos.ProductWeb;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Infrastructure.Repositories.ReadOnly
{
    public class ProductReadOnlyRepository : IProductReadOnlyRepository
    {
        private readonly IReadOnlyUnitOfWork _readOnlyUnitOfWork;
        private readonly IMapper _mapper;

        public ProductReadOnlyRepository(IReadOnlyUnitOfWork unitOfWork, IMapper mapper)
        {
            _readOnlyUnitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<ProductCardDto>> GetRawHomeProductsAsync(DateTime thirtyDaysAgo)
        {
            var today = DateTime.UtcNow.Date;
            return await _readOnlyUnitOfWork.Repository<Product>()
				.Entities
				.Where(x => x.Status == (byte)EntityStatus.Active && x.PublishDate <= today && x.PublishDate > thirtyDaysAgo && x.IsDeleted != true)
                .OrderByDescending(x => x.PublishDate)
                .Take(100)
                .AsNoTracking()
                .ProjectTo<ProductCardDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<(List<ProductCardDto> Items, int Total)> GetPagedProductsAsync(ProductQueryParameters query)
        {
            var queryable = _readOnlyUnitOfWork.Repository<Product>()
				.Entities
				.Where(x => x.Status == (byte)EntityStatus.Active && x.PublishDate <= DateTime.UtcNow && x.IsDeleted != true);

            // Filter logic (Category, Brand, Search) giữ nguyên nhưng thực thi tại Repo
            if (!string.IsNullOrEmpty(query.CategorySlug))
                queryable = queryable.Where(x => x.ProductCategoryMappings.Any(m => m.Category.NameAscii == query.CategorySlug));

            if (!string.IsNullOrEmpty(query.BrandSlug))
                queryable = queryable.Where(x => x.Brand.NameAscii == query.BrandSlug);

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                var search = query.SearchTerm.Trim().ToLower();
                queryable = queryable.Where(x => x.Name.ToLower().Contains(search) || x.Sku.ToLower().Contains(search));
            }

            int total = await queryable.CountAsync();
            var items = await queryable
                .OrderByDescending(x => x.CreatedAt)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ProjectTo<ProductCardDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return (items, total);
        }

        public async Task<ProductDetailDto?> GetProductDetailWithVariantsAsync(string slug, string version, bool isDefault)
        {
            return await _readOnlyUnitOfWork.Repository<Product>()
				.Entities
				.Where(x => x.NameAscii == slug && x.IsDeleted != true)
                .Include(x => x.ProductVariants.Where(y => y.NameAscii.ToLower() == version.ToLower() || (y.IsDefault == isDefault && y.IsActive == true)))
                .ProjectTo<ProductDetailDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }

        public async Task<List<AttributeGroupDto>> GetProductAttributesAsync(int productId)
        {
            var rawAttributes = await _readOnlyUnitOfWork.Repository<ProductAttributeValue>()
				.Entities
				.Where(x => x.ProductId == productId)
                .Select(a => new { Group = a.Attribute.Group.Name, Key = a.Attribute.Name, Val = a.Value })
                .ToListAsync();

            return rawAttributes.GroupBy(a => a.Group)
                .Select(g => new AttributeGroupDto
                {
                    GroupName = g.Key,
                    Attributes = g.Select(x => new AttributeValueDto { Key = x.Key, Value = x.Val }).ToList()
                }).ToList();
        }

        public async Task<Category?> GetMainCategoryByProductIdAsync(int productId)
        {
            return await _readOnlyUnitOfWork.Repository<ProductCategoryMapping>()
				.Entities
				.Where(m => m.ProductId == productId && m.IsMain == true)
                .Select(m => m.Category)
                .FirstOrDefaultAsync();
        }

        public async Task<List<BreadcrumbDto>> GetBreadcrumbsAsync(int productId)
        {
            var breadcrumbs = new List<BreadcrumbDto>();

            // Lấy Category chính
            var currentCat = await _readOnlyUnitOfWork.Repository<ProductCategoryMapping>()
				.Entities
				.Where(m => m.ProductId == productId && m.IsMain == true)
                .Select(m => m.Category)
                .FirstOrDefaultAsync();

            while (currentCat != null)
            {
                breadcrumbs.Insert(0, new BreadcrumbDto
                {
                    Name = currentCat.Name,
                    NameAscii = currentCat.NameAscii
                });

                if (!currentCat.ParentId.HasValue || currentCat.ParentId == 0) break;

                currentCat = await _readOnlyUnitOfWork.Repository<Category>()
				.Entities
				.Where(c => c.Id == currentCat.ParentId)
                    .FirstOrDefaultAsync();
            }

            return breadcrumbs;
        }
    }
}
