using Ecom.ProductService.Core.Abstractions.Persistence;
using Ecom.ProductService.Core.Entities;
using Ecom.ProductService.Core.Enums;
using Ecom.Shared.Grpc;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ecom.ProductService.Application.Service.Web
{
    // Chỉ comment dòng quan trọng: Kế thừa class base do gRPC tự sinh ra từ file proto
    public class OrderProductService : ProductGrpc.ProductGrpcBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrderProductService> _logger;

        public OrderProductService(IUnitOfWork unitOfWork, ILogger<OrderProductService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // Chỉ comment dòng quan trọng: Override hàm GetProductDisplayInfos để xử lý logic lấy data
        public override async Task<ProductResponse> GetProductDisplayInfos(ProductRequest request, ServerCallContext context)
        {
            var response = new ProductResponse();

            if (request.Items.Count == 0) return response;

            // Truy vấn danh sách sản phẩm theo Ids
            // Tách danh sách ID ra trước để EF Core có thể dịch sang câu lệnh SQL "IN (...)"
            var productIds = request.Items.Select(x => x.Id).Distinct().ToList();
            var variantIds = request.Items.Select(x => x.VariantId).Distinct().ToList();

            // Chỉ comment dòng quan trọng: Lấy Product và lọc Variant theo danh sách ID đã tách
            var products = await _unitOfWork.Repository<Product>()
                .GetAll(x => productIds.Contains(x.Id))
                .Include(x => x.ProductVariants.Where(v => variantIds.Contains(v.Id)))
                .ToListAsync();

            // Chuyển đổi dữ liệu sang định dạng gRPC Response

            // Chỉ comment dòng quan trọng: Sử dụng SelectMany để làm phẳng danh sách Variant thành một list ProductInfo duy nhất
            var productInfos = products.SelectMany(v => v.ProductVariants.Select(p => new ProductInfo
            {
                Id = v.Id,             // ID của sản phẩm chính
                VariantId = p.Id,      // ID của phiên bản (phải có trong file .proto mới của ông)
                Name = v.VersionName,
                VariantName = p.ColorName ?? "", // Tránh null để gRPC không crash
                Price = (double)p.Price,
                ImageUrl = v.MainImage ?? "",
                CurrencyUnit = v.CurrencyUnit ?? "VNĐ",
                IsAvailable = v.Status == (byte)EntityStatus.Active && (p.IsActive ?? false) // Kiểm tra cả trạng thái sản phẩm và phiên bản
            }));

            // Bây giờ AddRange sẽ chạy mượt vì productInfos đã là IEnumerable<ProductInfo>
            response.Products.AddRange(productInfos);
            return response;
        }

        public override async Task<ProductResponse> GetProductCheckoutDetails(ProductRequest request, ServerCallContext context)
        {
            var response = new ProductResponse();

            var productIds = request.Items.Select(x => x.Id).Distinct().ToList();
            var variantIds = request.Items.Select(x => x.VariantId).Distinct().ToList();

            // Truy vấn Database: Lấy Product + Variants + Images liên quan
            var products = await _unitOfWork.Repository<Product>()
                .GetAll(x => productIds.Contains(x.Id))
                .Include(x => x.ProductVariants.Where(v => variantIds.Contains(v.Id)))
                .Include(x => x.ProductImages)
                .ToListAsync();

            foreach (var item in request.Items)
            {
                var product = products.FirstOrDefault(p => p.Id == item.Id);
                var variant = product?.ProductVariants.FirstOrDefault(v => v.Id == item.VariantId);
                var isProductActive = product?.Status == (byte)EntityStatus.Active;
                var isVariantActive = variant?.IsActive == true;
                if (product != null && variant != null)
                {
                    //Ưu tiên lấy ảnh theo VariantId, nếu NULL thì lấy ảnh đầu tiên của Product
                    var variantImage = product.ProductImages.FirstOrDefault(img => img.VariantId == variant.Id)?.ImagePath
                                       ?? product.MainImage;

                    response.Products.Add(new ProductInfo
                    {
                        Id = product.Id,
                        VariantId = variant.Id,
                        Name = product.Name,
                        VariantName = variant.ColorName,
                        Price = (double)variant.Price, // Giá chính xác của phiên bản 
                        ImageUrl = variantImage ?? "",
                        CurrencyUnit = product.CurrencyUnit ?? "VNĐ",
                        Sku = variant.Sku,
                        IsAvailable = isProductActive && isVariantActive
                    });
                }
            }

            return response;
        }
    }
    
}