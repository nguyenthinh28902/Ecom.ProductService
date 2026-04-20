using AutoMapper;
using Ecom.ProductService.Core.Entities;
using Ecom.ProductService.Core.Models.Dtos.Brand;
using Ecom.ProductService.Core.Models.Dtos.Navigation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Core.AutoMappings
{
    public class BrandProfile : Profile
    {
        public BrandProfile()
        {
            // Map từ Request sang Entity để Save
            CreateMap<BrandDto, Brand>()
                .ForMember(dest => dest.LogoUrl, opt => opt.Ignore()) // Xử lý upload file riêng rồi gán path sau
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now));

            // Map từ Entity sang Response để trả về Client
            CreateMap<CreateBrandDto, Brand>();
            CreateMap<Brand, BrandResponse>();
        }
    }
}
