using Ecom.ProductService.Core.Abstractions.Persistence;
using Ecom.ProductService.Core.Abstractions.Persistence.Write;
using Ecom.ProductService.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Infrastructure.Repositories.Write
{
    public class BrandRepository : IBrandRepository
    {
        private readonly IUnitOfWork _unitOfWork;

        public BrandRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Kiểm tra NameAscii đã tồn tại hay chưa để tránh trùng lặp khi thêm mới hoặc cập nhật thương hiệu
        /// </summary>
        /// <param name="NameAscii">Tên ASCII của thương hiệu cần kiểm tra</param>
        /// <returns>True nếu NameAscii đã tồn tại, ngược lại False</returns>
        public async Task<bool> CheckNameAscii(string NameAscii)
        {
            var existingBrand = await _unitOfWork.Repository<Brand>().EntitiesNoTracking.FirstOrDefaultAsync(b => b.NameAscii == NameAscii);
            return existingBrand != null;
        }
    }
}
