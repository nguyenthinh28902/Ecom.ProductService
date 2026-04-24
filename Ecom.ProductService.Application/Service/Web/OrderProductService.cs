using Ecom.ProductService.Core.Abstractions.Persistence;
using Ecom.ProductService.Core.Abstractions.Persistence.ReadOnly;
using Ecom.ProductService.Core.Entities;
using Ecom.ProductService.Core.Enums;
using Ecom.ProductService.Infrastructure.Repositories;
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
        private readonly IReadOnlyUnitOfWork _unitOfWork;
        public OrderProductService(IOrderReadOnlyRepository orderRepo, IReadOnlyUnitOfWork unitOfWork)
        {
            _orderRepo = orderRepo;
            _unitOfWork = unitOfWork;
        }

        public override async Task<ProductResponse> GetProductDisplayInfos(ProductRequest request, ServerCallContext context)
        {
            var response = new ProductResponse();
            if (request.Items.Count == 0) return response;

            var productIds = request.Items.Select(x => x.Id).Distinct().ToList();
            var variantIds = request.Items.Select(x => x.VariantId).Distinct().ToList();

           
            var productVariants = await _unitOfWork.Repository<ProductVariant>()
                .Entities
                .AsNoTracking() // Thêm cái này vì ný chỉ đọc, giúp tăng tốc độ truy vấn
                .Include(x => x.Product)
                .Where(v => variantIds.Contains(v.Id) && v.IsDeleted != true && v.Product.IsDeleted != true)
                 // Sửa x thành v ở đây ný ơi
                .ToListAsync();
            if (productVariants == null || !productVariants.Any())
            {
                // Ném lỗi NotFound của gRPC để service khác biết đường mà xử lý
                throw new RpcException(new Status(StatusCode.NotFound, "Không tìm thấy sản phẩm nào hợp lệ để thanh toán ơi!"));
            }
            foreach (var item in productVariants)
            {
                var productInfo = new ProductInfo();
                productInfo = new ProductInfo
                {
                    Id = item.ProductId,                     // ID của Product cha
                    VariantId = item.Id,              // ID của Variant con
                    Name = item.Product.Name + " - " + item.ColorName,                 // Dùng v.Name (thường là tên sản phẩm chính)
                    VariantName = item.ColorName,   // Đảm bảo dùng đúng field chứa tên version (như "Xanh/Size L")
                    Price = (double)item.Price,
                    ImageUrl = item.Product.MainImage ?? "", // Ưu tiên ảnh của Variant, không có mới lấy ảnh đại diện Product
                    IsAvailable = item.Product.Status == (byte)EntityStatus.Active, // Giả định 1 là Active
                    CurrencyUnit = "VNĐ",
                    Sku = item.Sku ?? ""
                };
                response.Products.Add(productInfo);
            }
            
            return response;
        }

        public override async Task<ProductResponse> GetProductCheckoutDetails(ProductRequest request, ServerCallContext context)
        {
            var response = new ProductResponse();
            var productIds = request.Items.Select(x => x.Id).Distinct().ToList();
            var variantIds = request.Items.Select(x => x.VariantId).Distinct().ToList();

            var products = await _orderRepo.GetProductsForCheckoutAsync(productIds, variantIds);
            if (products == null || !products.Any())
            {
                // Ném lỗi NotFound của gRPC để service khác biết đường mà xử lý
                throw new RpcException(new Status(StatusCode.NotFound, "Không tìm thấy sản phẩm nào hợp lệ để thanh toán ơi!"));
            }
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