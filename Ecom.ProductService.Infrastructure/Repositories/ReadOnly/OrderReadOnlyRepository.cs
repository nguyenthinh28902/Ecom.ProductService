using Ecom.ProductService.Core.Abstractions.Persistence;
using Ecom.ProductService.Core.Abstractions.Persistence.ReadOnly;
using Ecom.ProductService.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Infrastructure.Repositories.ReadOnly
{
    public class OrderReadOnlyRepository : IOrderReadOnlyRepository
    {
        private readonly IReadOnlyUnitOfWork _readOnlyUnitOfWork;
        public OrderReadOnlyRepository(IReadOnlyUnitOfWork unitOfWork) => _readOnlyUnitOfWork = unitOfWork;

        public async Task<List<Product>> GetProductsWithVariantsAsync(List<int> productIds, List<int> variantIds)
        {
            return await _readOnlyUnitOfWork.Repository<Product>()
                        .Entities
                        .AsNoTracking() // Thêm cái này vì ný chỉ đọc, giúp tăng tốc độ truy vấn
                        .Where(x => productIds.Contains(x.Id) && x.IsDeleted != true)
                        .Include(x => x.ProductVariants.Where(v => variantIds.Contains(v.Id) && v.IsDeleted != true)) // Sửa x thành v ở đây ný ơi
                        .ToListAsync();
        }

        public async Task<List<Product>> GetProductsForCheckoutAsync(List<int> productIds, List<int> variantIds)
        {
            return await _readOnlyUnitOfWork.Repository<Product>().Entities
                .Where(x => productIds.Contains(x.Id) && x.IsDeleted != true)
                .Include(x => x.ProductVariants.Where(v => variantIds.Contains(v.Id) && v.IsDeleted != true))
                .Include(x => x.ProductImages.Where(img => img.IsDeleted != true))
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
