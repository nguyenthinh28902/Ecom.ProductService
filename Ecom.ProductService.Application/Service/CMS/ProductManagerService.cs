using AutoMapper;
using Ecom.Contracts.ProductService;
using Ecom.ProductService.Application.Interface.Auth;
using Ecom.ProductService.Application.Interface.CMS;
using Ecom.ProductService.Core.Abstractions.Persistence;
using Ecom.ProductService.Core.Entities;
using Ecom.ProductService.Core.Enums;
using Ecom.ProductService.Core.Exceptions;
using Ecom.ProductService.Core.Models;
using Ecom.ProductService.Core.Models.Auth;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Application.Service.CMS
{
    public class ProductManagerService : IProductManagerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IBaseService _baseService;
        private readonly ISystemLogManagerService _systemLogManagerService;
        private readonly ILogger<ProductManagerService> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        public ProductManagerService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
             IBaseService baseService,
             ISystemLogManagerService systemLogManagerService,
             ILogger<ProductManagerService> logger,
             IPublishEndpoint publishEndpoint)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _baseService = baseService;
            _systemLogManagerService = systemLogManagerService;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        /// <summary>
        /// Xóa sản phẩm
        /// </summary>
        /// <param name="productId">id sản phẩm</param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        public async Task<Result<bool>> ProductDelete(int productId)
        {
            // 0. Kiểm tra quyền hạn
            _baseService.EnsurePermission(ProductPermission.ProductDelete);
            _logger.LogInformation("Bắt đầu quy trình xóa sản phẩm: {ProductId}", productId);

            // 1. Kiểm tra sản phẩm tồn tại (Fail-Fast)
            //nên dùng Repo để lấy Product kèm theo ProductVariants
            var product = await _unitOfWork.Repository<Product>().Entities
                .Include(p => p.ProductVariants)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductAttributeValues)
                .FirstOrDefaultAsync(p => p.Id == productId && p.IsDeleted != true);

            if (product == null)
            {
                _logger.LogWarning("Không tìm thấy sản phẩm {ProductId} cần xóa.", productId);
                throw new NotFoundException($"Sản phẩm có ID {productId} không tồn tại!");
            }

            // 2. Bắt đầu Transaction
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 3. Xóa các thông tin liên quan (Soft Delete)

                // Xóa ProductVariants
                if (product.ProductVariants.Any())
                {
                    foreach (var variant in product.ProductVariants)
                    {
                        variant.IsDeleted = true;
                        variant.UpdatedAt = DateTime.Now;
                    }
                    _logger.LogDebug("Đã đánh dấu xóa {Count} variants của sản phẩm {ProductId}", product.ProductVariants.Count, productId);
                }

                // Xóa Hình ảnh sản phẩm
                if (product.ProductImages.Any())
                {
                    foreach (var img in product.ProductImages)
                    {
                        img.IsDeleted = true;
                    }
                    var systemLogProductImages = _systemLogManagerService.CreateSystemLog();
                    systemLogProductImages.TableName = nameof(ProductImage);
                    systemLogProductImages.RecordId = productId;
                    systemLogProductImages.ColumnName = "IsDeleted";
                    systemLogProductImages.OldValue = $"{product.ProductImages.Count()} records";
                    systemLogProductImages.NewValue = $"0 records";
                    systemLogProductImages.Action = SystemAction.Delete;
                    systemLogProductImages.FunctionName = nameof(ProductDelete);
                    await _unitOfWork.Repository<SystemLog>().AddAsync(systemLogProductImages);
                }

                // Xóa các Attribute Values (thường là bảng trung gian nên có thể xóa hẳn hoặc flag)
                if (product.ProductAttributeValues.Any())
                {
                    foreach (var attr in product.ProductAttributeValues)
                    {
                        attr.IsDeleted = true;
                    }
                    var systemLogProductAttributeValues = _systemLogManagerService.CreateSystemLog();
                    systemLogProductAttributeValues.TableName = nameof(ProductAttributeValue);
                    systemLogProductAttributeValues.RecordId = productId;
                    systemLogProductAttributeValues.ColumnName = "IsDeleted";
                    systemLogProductAttributeValues.OldValue = $"{product.ProductAttributeValues.Count()} records";
                    systemLogProductAttributeValues.NewValue = $"0 records";
                    systemLogProductAttributeValues.FunctionName = nameof(ProductDelete);
                    systemLogProductAttributeValues.Action = SystemAction.Delete;
                    await _unitOfWork.Repository<SystemLog>().AddAsync(systemLogProductAttributeValues);
                }

                // 4. Xóa Product chính
                product.IsDeleted = true;
                product.Status = 0; // Chuyển trạng thái về Inactive (giả định byte 0 là Inactive)
                product.UpdatedAt = DateTime.Now;

                // 5. Ghi Log hệ thống (SystemLog) tương tự BrandManagerService
                var systemLog = _systemLogManagerService.CreateSystemLog();
                systemLog.TableName = nameof(Product);
                systemLog.RecordId = productId;
                systemLog.ColumnName = "IsDeleted, Status";
                systemLog.OldValue = $"IsDeleted: false, Status: {product.Status}";
                systemLog.NewValue = $"IsDeleted: true, Status: 0";
                systemLog.Action = SystemAction.Delete;
                systemLog.FunctionName = nameof(ProductDelete);
                await _unitOfWork.Repository<SystemLog>().AddAsync(systemLog);

                // 6. Lưu thay đổi và Commit
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                await PublishOrderCompletedEvent(productId, null);

                _logger.LogInformation("Xóa sản phẩm {ProductId} thành công!", productId);
                return Result<bool>.Success(true, "Xóa sản phẩm thành công!");
            }
            catch (Exception ex)
            {
                // Rollback nếu có bất kỳ lỗi nào xảy ra
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi xóa sản phẩm {ProductId}", productId);
                throw;
            }
        }


        /// <summary>
        /// Xóa version sản phẩm
        /// </summary>
        /// <param name="variantId">id Version sản phẩm</param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        public async Task<Result<bool>> ProductVariantDelete(int variantId)
        {
            
            _baseService.EnsurePermission(ProductPermission.ProductUpdate);
            _logger.LogInformation("Bắt đầu xóa biến thể sản phẩm Id: {VariantId}", variantId);

          
            var variant = await _unitOfWork.Repository<ProductVariant>()
            .Entities
            .Include(v => v.Product.ProductImages.Where(img => img.VariantId == variantId && img.IsDeleted != true))
            .FirstOrDefaultAsync(v => v.Id == variantId && v.IsDeleted != true);

            if (variant == null)
            {
                throw new NotFoundException($"Biến thể sản phẩm không tồn tại ný ơi!");
            }

            
            var variantImages = variant.Product.ProductImages;

            
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if(variantImages.Any())
                {
                    foreach (var image in variantImages)
                    {
                        image.IsDeleted = true;
                        
                    }
                    var systemLogProductImages = _systemLogManagerService.CreateSystemLog();
                    systemLogProductImages.TableName = nameof(ProductImage);
                    systemLogProductImages.RecordId = variantId;
                    systemLogProductImages.ColumnName = "IsDeleted";
                    systemLogProductImages.OldValue = $"{variantImages.Count} records";
                    systemLogProductImages.NewValue = $"0 records";
                    systemLogProductImages.FunctionName = nameof(ProductVariantDelete);
                    systemLogProductImages.Action = SystemAction.Delete;
                    await _unitOfWork.Repository<SystemLog>().AddAsync(systemLogProductImages);
                }
                // 3. Thực hiện Soft Delete
                variant.IsDeleted = true;
                variant.IsActive = false; // Ngắt kích hoạt luôn cho chắc
                variant.UpdatedAt = DateTime.Now;

                // 4. Ghi System Log (Giống cách làm bên BrandManager)
                var systemLog = _systemLogManagerService.CreateSystemLog();
                systemLog.TableName = nameof(ProductVariant);
                systemLog.RecordId = variantId;
                systemLog.ColumnName = "IsDeleted, IsActive";
                systemLog.OldValue = $"IsDeleted: false, IsActive: {variant.IsActive}";
                systemLog.NewValue = $"IsDeleted: true, IsActive: false";
                systemLog.Action = SystemAction.Delete;
                systemLog.FunctionName = nameof(ProductVariantDelete);
                await _unitOfWork.Repository<SystemLog>().AddAsync(systemLog);

                // 5. Lưu và Commit
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                await PublishOrderCompletedEvent(variant.ProductId, variantId);
                _logger.LogInformation("Xóa biến thể {VariantId} thành công", variantId);
                return Result<bool>.Success(true, "Xóa biến thể sản phẩm thành công!");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi xóa biến thể {VariantId}", variantId);
                throw;
            }
        }

        private async Task PublishOrderCompletedEvent(int productId, int? productVariantId)
        {
            try
            {
                var orderCompletedEvent = new OrderCompletedEvent
                {
                    ProductId = productId,
                    ProductVariantId = productVariantId
                };
                await _publishEndpoint.Publish(orderCompletedEvent);
                _logger.LogInformation("Đã đẩy tin nhắn xóa giỏ hàng lên RabbitMQ thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError("{PublishOrderCompletedEvent} gọi event lỗi {Message}", nameof(PublishOrderCompletedEvent), ex.Message);
            }
        }
    }
}
