using AutoMapper;
using Ecom.ProductService.Application.Interface.Auth;
using Ecom.ProductService.Application.Interface.CMS;
using Ecom.ProductService.Application.Service.Auth;
using Ecom.ProductService.Core.Abstractions.Persistence;
using Ecom.ProductService.Core.Abstractions.Persistence.Write;
using Ecom.ProductService.Core.Entities;
using Ecom.ProductService.Core.Exceptions;
using Ecom.ProductService.Core.Models;
using Ecom.ProductService.Core.Models.Auth;
using Ecom.ProductService.Core.Models.Dtos.Brand;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace Ecom.ProductService.Application.Service.CMS
{
    public class BrandManagerService : IBrandManagerService
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<BrandManagerService> _logger;
        private readonly IBaseService _baseService;
        private readonly ISystemLogManagerService _systemLogManagerService;
        public BrandManagerService(
            IBrandRepository brandRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
             ILogger<BrandManagerService> logger,
             IBaseService baseService,
             ISystemLogManagerService systemLogManagerService)
        {
            _brandRepository = brandRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _baseService = baseService;
            _systemLogManagerService = systemLogManagerService;
        }
        public async Task<Result<BrandResponse>> CreateBrandAsync(CreateBrandDto request)
        {
            _baseService.EnsurePermission(ProductPermission.ProductCreate);
            // Log thông tin bắt đầu nghiệp vụ - dùng Structured Logging để sau này search theo BrandName
            _logger.LogInformation("Bắt đầu quy trình thêm thương hiệu mới: {BrandName}", request.Name);

            // 1. Kiểm tra đầu vào cơ bản
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                _logger.LogWarning("Thêm thương hiệu thất bại: Tên bị trống.");
                throw new BadRequestException("Tên thương hiệu không được để trống ný ơi!");
            }
            //Check điều kiện CheckNameAscii
            if (await _brandRepository.CheckNameAscii(request.NameAscii))
            {
                _logger.LogWarning("Thêm thương hiệu thất bại: Tên ASCII đã tồn tại.");
                throw new ConflictException("Tên thương hiệu hoặc mã định danh đã tồn tại rồi ný!");
            }
            // tạo transaction để đảm bảo nếu có lỗi ở bước nào thì sẽ rollback về trạng thái ban đầu, tránh dữ liệu bị dơ
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 2. Map DTO sang Entity
                var brand = _mapper.Map<Brand>(request);
                brand.LogoUrl = string.Empty;

                // 3. Thực hiện lưu vào Master DB
                
                // Log trước khi xuống Repo để biết app đã chạy đến bước này
                _logger.LogDebug("Đang đẩy dữ liệu Brand {BrandName} xuống Database...", request.Name);
                await _unitOfWork.Repository<Brand>().AddAsync(brand);
                await _unitOfWork.SaveChangesAsync();
                // 4. Kiểm tra con trỏ sau khi lưu
                if (brand.Id == 0)
                {
                    _logger.LogError("Database không trả về Id cho Brand: {BrandName}", request.Name);
                    throw new Exception("Database không trả về Id mới.");
                }
                var systemLog = _systemLogManagerService.CreateSystemLog();
                systemLog.TableName = nameof(Brand);
                systemLog.RecordId = brand.Id;
                systemLog.Action = "Create";
                await _unitOfWork.Repository<SystemLog>().AddAsync(systemLog);
                await _unitOfWork.CommitAsync();
                // 5. Trả về Result thành công
                _logger.LogInformation("Thêm thương hiệu thành công: {BrandName} (Id: {BrandId})", brand.Name, brand.Id);

                var brandResponse = _mapper.Map<BrandResponse>(brand);
                return Result<BrandResponse>.Success(brandResponse, "Thêm thương hiệu thành công!");
            }
            catch (Exception ex)
            {
                // CỨU TINH ĐÂY: Hủy bỏ mọi thay đổi nếu có lỗi xảy ra
                await _unitOfWork.RollbackAsync();

                _logger.LogError(ex, "Lỗi quy trình thêm Brand: {BrandName}", request.Name);
                throw; // Quăng lỗi ra cho Middleware xử lý tiếp
            }
        }
    }
}
