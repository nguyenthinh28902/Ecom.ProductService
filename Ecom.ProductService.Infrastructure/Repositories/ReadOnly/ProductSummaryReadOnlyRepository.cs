using Ecom.ProductService.Core.Abstractions.Persistence;
using Ecom.ProductService.Core.Abstractions.Persistence.ReadOnly;
using Ecom.ProductService.Core.Entities;
using Ecom.ProductService.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Infrastructure.Repositories.ReadOnly
{
    public class ProductSummaryReadOnlyRepository : IProductSummaryReadOnlyRepository
    {
        private readonly IReadOnlyUnitOfWork _readOnlyUnitOfWork;
        public ProductSummaryReadOnlyRepository(IReadOnlyUnitOfWork unitOfWork) => _readOnlyUnitOfWork = unitOfWork;

        public async Task<int> CountActiveBrandsAsync() =>
            await _readOnlyUnitOfWork.Repository<Brand>().CountAsync(x => x.IsDeleted != true);

        public async Task<int> CountActiveCategoriesAsync() =>
            await _readOnlyUnitOfWork.Repository<Category>().CountAsync(x => x.IsDeleted != true);

        public async Task<int> CountActiveProductsAsync() =>
            await _readOnlyUnitOfWork.Repository<Product>().CountAsync(x => x.IsDeleted != true && x.Status == (byte)EntityStatus.Active);

        public async Task<int> CountPendingProductsAsync(DateTime today) =>
            await _readOnlyUnitOfWork.Repository<Product>().CountAsync(x => x.IsDeleted != true && x.Status == (byte)EntityStatus.Active && x.PublishDate <= today);
    }
}
