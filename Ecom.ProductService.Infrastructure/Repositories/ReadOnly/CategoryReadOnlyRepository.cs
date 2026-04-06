using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecom.ProductService.Core.Abstractions.Persistence;
using Ecom.ProductService.Core.Abstractions.Persistence.ReadOnly;
using Ecom.ProductService.Core.Entities;
using Ecom.ProductService.Core.Enums;
using Ecom.ProductService.Core.Models.Dtos.Navigation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Infrastructure.Repositories.ReadOnly
{
    public class CategoryReadOnlyRepository : ICategoryReadOnlyRepository
    {
        private readonly IReadOnlyUnitOfWork _readOnlyUnitOfWork;
        private readonly IMapper _mapper;

        public CategoryReadOnlyRepository(IReadOnlyUnitOfWork unitOfWork, IMapper mapper)
        {
            _readOnlyUnitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<CategoryDto>> GetNavigationCategoriesAsync()
        {
            //  Tối ưu hóa truy vấn cho hệ thống Replication
            return await _readOnlyUnitOfWork.Repository<Category>()
                .GetAll(x => x.IsDeleted != true && x.Status == (byte)EntityStatus.Active)
                .AsNoTracking()
                .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }
    }
}
