using AutoMapper;
using Ecom.ProductService.Core.Entities;
using Ecom.ProductService.Core.Models.Dtos.Navigation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Application.AutoMappings
{
    public class ApplicationNavigationProfile : Profile
    {
        public ApplicationNavigationProfile() {

            // Mapping cho Brand
            CreateMap<Brand, BrandDto>();

            // Mapping cho Category (Xử lý đệ quy cho menu cha - con)
            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.SubCategories, opt => opt.MapFrom(src => src.InverseParent));
        }
    }
}
