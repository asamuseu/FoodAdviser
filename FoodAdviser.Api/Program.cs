using AutoMapper;
using FoodAdviser.Application.Mapping;
using FoodAdviser.Application.Options;
using FoodAdviser.Application.Services;
using FoodAdviser.Domain.Repositories;
using FoodAdviser.Infrastructure.Repositories;
using FoodAdviser.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(ReceiptProfile).Assembly);
builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));
builder.Services.Configure<ReceiptAnalyzerOptions>(builder.Configuration.GetSection("ReceiptAnalyzer"));
var connString = builder.Configuration.GetConnectionString("Default") ?? "Server=(localdb)\\MSSQLLocalDB;Database=FoodAdviser;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

builder.Services.AddScoped<IReceiptRepository, ReceiptRepository>();
builder.Services.AddHttpClient<ReceiptAnalyzerService>();

// Swap registration to use HttpClient-based constructor via typed client
builder.Services.AddScoped<IReceiptAnalyzerService>(sp => sp.GetRequiredService<ReceiptAnalyzerService>());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Infrastructure DI with explicit EF Core provider configuration
FoodAdviser.Infrastructure.DependencyInjection.ServiceCollectionExtensions.AddFoodAdviserInfrastructure(
    builder.Services,
    builder.Configuration,
    options => options.UseSqlServer(connString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
