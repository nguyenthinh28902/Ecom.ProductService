using Ecom.ProductService.Core.Abstractions.Persistence;
using Ecom.ProductService.Core.Abstractions.Persistence.ReadOnly;
using Ecom.ProductService.Core.Entities;
using Ecom.ProductService.Core.Enums;
using Ecom.Shared.Grpc;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ecom.ProductService.Application.Service.Web
{
    //  Kế thừa class base do gRPC tự sinh ra từ file proto
    public class OrderProductService : ProductGrpc.ProductGrpcBase
    {
        private readonly IOrderReadOnlyRepository _orderRepo;
        public OrderProductService(IOrderReadOnlyRepository orderRepo) => _orderRepo = orderRepo;

        public override async Task<ProductResponse> GetProductDisplayInfos(ProductRequest request, ServerCallContext context)
        {
            var response = new ProductResponse();
            if (request.Items.Count == 0) return response;

            var productIds = request.Items.Select(x => x.Id).Distinct().ToList();
            var variantIds = request.Items.Select(x => x.VariantId).Distinct().ToList();

            var products = await _orderRepo.GetProductsWithVariantsAsync(productIds, variantIds);

            var productInfos = products.SelectMany(v => v.ProductVariants.Select(p => new ProductInfo
            {
                Id = v.Id,
                VariantId = p.Id,
                Name = v.VersionName,
                VariantName = p.ColorName ?? "",
                Price = (double)p.Price,
                ImageUrl = v.MainImage ?? "",
                IsAvailable = v.Status == (byte)EntityStatus.Active && (p.IsActive ?? false)
            }));

            response.Products.AddRange(productInfos);
            return response;
        }

        public override async Task<ProductResponse> GetProductCheckoutDetails(ProductRequest request, ServerCallContext context)
        {
            var response = new ProductResponse();
            var productIds = request.Items.Select(x => x.Id).Distinct().ToList();
            var variantIds = request.Items.Select(x => x.VariantId).Distinct().ToList();

            var products = await _orderRepo.GetProductsForCheckoutAsync(productIds, variantIds);

            foreach (var item in request.Items)
            {
                var product = products.FirstOrDefault(p => p.Id == item.Id);
                var variant = product?.ProductVariants.FirstOrDefault(v => v.Id == item.VariantId);
                if (product != null && variant != null)
                {
                    var variantImage = product.ProductImages.FirstOrDefault(img => img.VariantId == variant.Id)?.ImagePath ?? product.MainImage;
                    response.Products.Add(new ProductInfo
                    {
                        Id = product.Id,
                        VariantId = variant.Id,
                        Name = product.Name,
                        VariantName = variant.ColorName,
                        Price = (double)variant.Price,
                        ImageUrl = variantImage ?? "",
                        IsAvailable = product.Status == (byte)EntityStatus.Active && variant.IsActive == true
                    });
                }
            }
            return response;
        }
    }
    
}