using AutoMapper;
using Ecom.ProductService.Core.Entities;
using Ecom.ProductService.Core.Enums;
using Ecom.ProductService.Core.Models.Dtos.ProductWeb;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Application.AutoMappings
{
    public class ApplicationProductWebProfile : Profile
    {
        public ApplicationProductWebProfile()
        {
            int? targetVariantId = null;
            CreateMap<Product, ProductCardDto>()
            .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand.Name))
            .ForMember(dest => dest.CategoryNameAscii, opt => opt.MapFrom(src => 
                src.ProductCategoryMappings
                    .Where(x => x.IsMain == true)
                    .Select(x => x.Category.NameAscii)
                    .FirstOrDefault())); // Lấy Name trước khi gọi FirstOrDefault giúp SQL dịch mượt hơn
                    CreateMap<ProductImage, ProductImageDto>();
            CreateMap<ProductVariant, ProductVariantDto>();

            // Map chính cho Product Detail
            CreateMap<Product, ProductDetailDto>()
             .ForMember(d => d.FullDescription, opt => opt.MapFrom(s => s.ProductGroup.Description))
             .ForMember(d => d.BrandName, opt => opt.MapFrom(s => s.Brand.Name))
             .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.Status == (byte)EntityStatus.Active || s.PublishDate <= DateTime.UtcNow))
             .ForMember(d => d.IsStockAvailable, opt => opt.MapFrom(s => s.Status == (byte)EntityStatus.Active))
            // Map List đơn giản
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src =>
            src.ProductImages.Where(img => img.VariantId == targetVariantId && img.IsDeleted != true)))
             .ForMember(d => d.Variants, opt => opt.MapFrom(s => s.ProductVariants.Where(x => x.IsActive == true && x.IsDeleted != true)))
             // Map GroupProducts (Các sản phẩm cùng Group)
             .ForMember(d => d.GroupProducts, opt => opt.MapFrom(s => s.ProductGroup.Products))
             // Lấy danh sách Attribute phẳng (Sau đó Service sẽ Group lại sau)
             .ForMember(d => d.Specifications, opt => opt.Ignore())
             .ForMember(d => d.Breadcrumbs, opt => opt.Ignore());

            // Mapping cho RelatedProductDto trong cùng Group
            CreateMap<Product, RelatedProductDto>()
                .ForMember(d => d.IsCurrent, opt => opt.Ignore()); // Sẽ set ở Service
        }
    }
}
