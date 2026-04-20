using AutoMapper;
using Ecom.ProductService.Application.Interface.Auth;
using Ecom.ProductService.Application.Interface.CMS;
using Ecom.ProductService.Application.Service.CMS;
using Ecom.ProductService.Core.Abstractions.Persistence;
using Ecom.ProductService.Core.Abstractions.Persistence.Write;
using Ecom.ProductService.Core.Entities;
using Ecom.ProductService.Core.Enums;
using Ecom.ProductService.Core.Exceptions;
using Ecom.ProductService.Core.Models.Dtos.Brand;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Text;

namespace Ecom.ProductService.Tests.Services
{
    public class BrandServiceTests
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<BrandManagerService> _logger;
        private readonly BrandManagerService _sut; // SUT = System Under Test (Đối tượng cần test)
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISystemLogManagerService _systemLogManagerService;
        private readonly IBaseService _baseService;
        private readonly IRepository<Brand> _brandRepoMock;
        private readonly IRepository<SystemLog> _systemLogRepoMock;
        public BrandServiceTests()
        {
            // 1. Tạo các bản "giả" cho Dependencies
            _brandRepository = Substitute.For<IBrandRepository>();
            _brandRepoMock = Substitute.For<IRepository<Brand>>();
            _systemLogRepoMock = Substitute.For<IRepository<SystemLog>>();
            _mapper = Substitute.For<IMapper>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _logger = Substitute.For<ILogger<BrandManagerService>>();
            _baseService = Substitute.For<IBaseService>();
            _systemLogManagerService = Substitute.For<ISystemLogManagerService>();

            // 2. Khởi tạo Service với các đồ giả đã tạo
            _sut = new BrandManagerService(_brandRepository, _unitOfWork, _mapper, _logger, _baseService, _systemLogManagerService);
        }

        [Fact]
        public async Task CreateBrandAsync_ShouldReturnSuccess_WhenDataIsValid()
        {
            // Arrange
            var createBrandDto = GetCreateBrandDtoSample();
            var brandEntity = GetBrandSample();
            var brandResponse = new BrandResponse { Id = brandEntity.Id };
            var sysTemLog = GetSystemLogSample(recordId: brandEntity.Id, action: "Create");
            // Mock Check logic: Tên chưa tồn tại
            _brandRepository.CheckNameAscii(createBrandDto.NameAscii).Returns(false);
            _systemLogManagerService.CreateSystemLog().Returns(sysTemLog);
            // Mock Mapper
            _mapper.Map<Brand>(createBrandDto).Returns(brandEntity);
            _mapper.Map<BrandResponse>(brandEntity).Returns(brandResponse);

            // Mock Unit of Work cho Repository Brand

            _unitOfWork.Repository<Brand>().Returns(_brandRepoMock);
            _unitOfWork.Repository<SystemLog>().Returns(_systemLogRepoMock);

            // Act
            var result = await _sut.CreateBrandAsync(createBrandDto);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data?.Id.Should().Be(brandEntity.Id);
            Assert.NotNull(result);
            Assert.True(result.IsSuccess); // hoặc result.IsSuccess tùy model
            Assert.NotNull(result.Data);


            // Verify các bước quan trọng
            await _unitOfWork.Received(1).BeginTransactionAsync(); // Phải mở giao dịch
            await _brandRepoMock.Received(1).AddAsync(Arg.Is<Brand>(b => b.Name == brandEntity.Name));
            await _systemLogRepoMock.Received(1).AddAsync(Arg.Is<SystemLog>(log => log.TableName == nameof(Brand) && log.RecordId == brandEntity.Id));
            await _unitOfWork.Received(1).SaveChangesAsync(); // Phải Save tạm để lấy Id

            // Verify ghi log đúng bảng và Id

            await _unitOfWork.Received(1).CommitAsync(); // Cuối cùng phải chốt hạ
        }

        [Fact]
        public async Task CreateBrandAsync_ShouldThrowConflictException_WhenNameExists()
        {
            // Arrange
            var dto = GetCreateBrandDtoSample();
            _brandRepository.CheckNameAscii(dto.NameAscii).Returns(true);

            // Act
            Func<Task> act = () => _sut.CreateBrandAsync(dto);

            // Assert
            await act.Should().ThrowAsync<ConflictException>()
                .WithMessage("Tên thương hiệu hoặc mã định danh đã tồn tại rồi ný!");

            // Verify: Tuyệt đối không mở Transaction hay gọi Add
            await _unitOfWork.DidNotReceive().BeginTransactionAsync();
            _unitOfWork.DidNotReceive().Repository<Brand>();
        }

        // Hàm tạo data mẫu cho Brand Entity
        public Brand GetBrandSample(int id = 1, string name = "Samsung")
        {
            return new Brand
            {
                Id = id,
                Name = name,
                NameAscii = name.ToLower().Replace(" ", "-"),
                LogoUrl = $"https://api.example.com/uploads/{name.ToLower()}.png",
                Origin = "South Korea",
                Status = (byte)EntityStatus.Active,
                IsDeleted = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }

        // Hàm tạo data mẫu cho DTO (Request từ Client)
        public CreateBrandDto GetCreateBrandDtoSample(string name = "Samsung")
        {
            var content = "Fake image content";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            return new CreateBrandDto
            {
                Name = name,
                NameAscii = name.ToLower().Replace(" ", "-"),
                Origin = "South Korea",
                Status = EntityStatus.Active,
                LogoFile = new FormFile(stream, 0, stream.Length, "LogoFile", "logo.png")
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "image/png"
                }
            };
        }

        public SystemLog GetSystemLogSample(int recordId = 1, string action = "Create")
        {
            return new SystemLog
            {
                UserId = 1,
                Id = 1,
                UserAgent = "UnitTestAgent/1.0",
                Ipaddress = "",
                TableName = nameof(Brand),
                RecordId = recordId,
                Action = action,
                CreatedAt = DateTime.Now
            };

        }
    }
}

