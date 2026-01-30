using FoodAdviser.Application.Mapping;
using FoodAdviser.Application.Options;
using FoodAdviser.Application.Services;
using FoodAdviser.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(ReceiptProfile).Assembly);
builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));
builder.Services.Configure<ReceiptAnalyzerOptions>(builder.Configuration.GetSection("ReceiptAnalyzer"));
builder.Services.Configure<OpenAiOptions>(builder.Configuration.GetSection("OpenAi"));
builder.Services.Configure<RecipeSuggestionOptions>(builder.Configuration.GetSection("RecipeSuggestion"));

// Register application services
builder.Services.AddScoped<IReceiptService, ReceiptService>();
builder.Services.AddScoped<IReceiptAnalyzerService, ReceiptAnalyzerService>();
builder.Services.AddScoped<IRecipeSuggestionService, RecipeSuggestionService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();

// Register HttpClient for OpenAI service
builder.Services.AddHttpClient<IOpenAiService, OpenAiService>();

var connString = builder.Configuration.GetConnectionString("Default") ?? "Server=(localdb)\\MSSQLLocalDB;Database=FoodAdviser;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// FluentValidation: automatic validation and validator discovery
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<FoodAdviser.Api.DTOs.Receipts.Validators.UploadReceiptDtoValidator>();

// Infrastructure DI with explicit EF Core provider configuration
FoodAdviser.Infrastructure.DependencyInjection.ServiceCollectionExtensions.AddFoodAdviserInfrastructure(
    builder.Services,
    builder.Configuration,
    options => options.UseSqlServer(connString));

var app = builder.Build();

// Storage directory bootstrap: ensure exists and is writable
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    var storageOptions = scope.ServiceProvider.GetRequiredService<IOptions<StorageOptions>>().Value;

    if (string.IsNullOrWhiteSpace(storageOptions.ReceiptTempPath))
    {
        logger.LogError("StorageOptions.ReceiptTempPath is not configured. Please set Storage:ReceiptTempPath in appsettings.");
    }
    else
    {
        try
        {
            if (!Directory.Exists(storageOptions.ReceiptTempPath))
            {
                Directory.CreateDirectory(storageOptions.ReceiptTempPath);
                logger.LogInformation("Created storage directory at {Path}.", storageOptions.ReceiptTempPath);
            }

            // Verify write permissions by creating and deleting a temp file
            var testFile = Path.Combine(storageOptions.ReceiptTempPath, $".write-test-{Guid.NewGuid():N}.tmp");
            await File.WriteAllTextAsync(testFile, "test");
            File.Delete(testFile);
            logger.LogInformation("Verified write access to storage directory at {Path}.", storageOptions.ReceiptTempPath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to ensure storage directory at {Path}. Check permissions.", storageOptions.ReceiptTempPath);
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
