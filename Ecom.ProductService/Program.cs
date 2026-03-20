using Ecom.ProductService.Application.Service.Web;
using Ecom.ProductService.Common.DependencyInjection;
using Ecom.ProductService.Common.Extensions;
using Ecom.ProductService.Common.Helpers;
using Ecom.ProductService.Controllers.Web;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);
Console.OutputEncoding = System.Text.Encoding.UTF8;
// Add services to the container.
builder.Services.AddControllers();
//configure appsettings
builder.Services.AddConfigAppSettingExtensions(builder.Configuration);
//configure swagger gen
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSwaggerGenConfiguration(builder.Configuration);
//Authentication
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
builder.Services.AddAuthenticationExtensions(builder.Configuration);
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// apication DI
var loggerFactory = LoggerFactory.Create(b => b.AddConfiguration(builder.Configuration));
builder.Services.AddApplicationDI(builder.Configuration, loggerFactory);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.DisplayRequestDuration());
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();


app.UseAuthorization();

app.MapControllers();

app.MapGrpcService<OrderProductService>(); 

app.Run();
