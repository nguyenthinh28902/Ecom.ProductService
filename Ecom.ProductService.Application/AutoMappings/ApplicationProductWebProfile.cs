using AutoMapper;
using Ecom.ProductService.Core.Entities;
using Ecom.ProductService.Core.Models.Dtos.ProductWeb;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Application.AutoMappings
{
    public class ApplicationProductWebProfile : Profile
    {
        public ApplicationProductWebProfile() {

            CreateMap<Product, ProductCardDto>()
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand.Name));
        }
    }
}
