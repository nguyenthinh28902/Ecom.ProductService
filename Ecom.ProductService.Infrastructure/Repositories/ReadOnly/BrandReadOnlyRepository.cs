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
    public class BrandReadOnlyRepository : IBrandReadOnlyRepository
    {
        private readonly IReadOnlyUnitOfWork _readOnlyUnitOfWork;
        private readonly IMapper _mapper;

        public BrandReadOnlyRepository(IReadOnlyUnitOfWork unitOfWork, IMapper mapper)
        {
            _readOnlyUnitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<BrandDto>> GetNavigationBrandsAsync()
        {
            //  Thực hiện ProjectTo ngay tại đây để SQL chỉ Select các cột cần thiết
            return await _readOnlyUnitOfWork.Repository<Brand>()
                .GetAll(x => x.IsDeleted != true && x.Status == (byte)EntityStatus.Active)
                .AsNoTracking()
                .ProjectTo<BrandDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }
    }
}
