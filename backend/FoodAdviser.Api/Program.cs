using FoodAdviser.Application.Mapping;
using FoodAdviser.Application.Options;
using FoodAdviser.Application.Services;
using FoodAdviser.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// CORS policy for frontend development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:5174",
                "http://127.0.0.1:5173",
                "http://127.0.0.1:5174")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(ReceiptProfile).Assembly);
builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));
builder.Services.Configure<ReceiptAnalyzerOptions>(builder.Configuration.GetSection("ReceiptAnalyzer"));
builder.Services.Configure<OpenAiOptions>(builder.Configuration.GetSection("OpenAi"));
builder.Services.Configure<AiProviderOptions>(builder.Configuration.GetSection("AiProvider"));
builder.Services.Configure<RecipeSuggestionOptions>(builder.Configuration.GetSection("RecipeSuggestion"));

// Register application services
builder.Services.AddScoped<IReceiptService, ReceiptService>();
builder.Services.AddScoped<IReceiptAnalyzerService, ReceiptAnalyzerService>();
builder.Services.AddScoped<IRecipeSuggestionService, RecipeSuggestionService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();

// Register AI recipe services and factory
builder.Services.AddHttpClient<OpenAiService>();
builder.Services.AddScoped<IAiRecipeServiceFactory, AiRecipeServiceFactory>();

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
    options => options.UseNpgsql(connString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null);
        npgsqlOptions.CommandTimeout(60);
    }));

var app = builder.Build();

// Apply pending EF Core migrations automatically at startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<FoodAdviser.Infrastructure.Persistence.FoodAdviserDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
        logger.LogInformation("Applying database migrations...");
        db.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
        logger.LogError(ex, "Failed to apply database migrations. Ensure PostgreSQL is running and accessible at the configured connection string.");
        // Optionally rethrow if you want the app to fail fast when DB is unavailable
        // throw;
    }
}

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

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
