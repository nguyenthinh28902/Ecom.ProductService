using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecom.ProductService.Application.Interface.Web;
using Ecom.ProductService.Core.Abstractions.Persistence;
using Ecom.ProductService.Core.Entities;
using Ecom.ProductService.Core.Enums.Product;
using Ecom.ProductService.Core.Models;
using Ecom.ProductService.Core.Models.Dtos.ProductWeb;
using Microsoft.EntityFrameworkCore;
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
        public ProductWebService(ILogger<ProductWebService> logger
            , IUnitOfWork unitOfWork
            ,IMapper mapper)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<HomeProductDisplayDto>> GetProductHome()
        {
            _logger.LogInformation($"{nameof(GetProductHome)} start: ");
            var products = _unitOfWork.Repository<Product>()
                .GetAll(x=> x.Status == (byte)ProductStatus.Active && x.PublishDate >= DateTime.UtcNow.Date);
            var NewArrivals = await products.Where(x => x.PublishDate.HasValue && x.PublishDate.Value > DateTime.Now.AddDays(-30)).ProjectTo<ProductCardDto>(_mapper.ConfigurationProvider).ToListAsync();
            var bestsellers = await products
                .OrderByDescending(p => p.Price).Take(5)
                .ProjectTo<ProductCardDto>(_mapper.ConfigurationProvider).ToListAsync();

            var homeProduts = new HomeProductDisplayDto();
            homeProduts.NewArrivals = NewArrivals;
            homeProduts.BestDeals = bestsellers;
            _logger.LogInformation($"{nameof(GetProductHome)} end: ");
            return Result<HomeProductDisplayDto>.Success(homeProduts,"Danh sách sản phẩm trang chủ");
        }
    }
}
