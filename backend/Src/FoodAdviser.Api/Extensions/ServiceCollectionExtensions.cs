using FluentValidation;
using FluentValidation.AspNetCore;
using FoodAdviser.Api.Validators.Receipts;
using FoodAdviser.Application.Mapping;
using FoodAdviser.Application.Options;
using FoodAdviser.Application.Services.Interfaces;
using FoodAdviser.Application.Services;
using FoodAdviser.Infrastructure.DependencyInjection;
using FoodAdviser.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace FoodAdviser.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
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

        return services;
    }

    public static IServiceCollection AddApplicationOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<StorageOptions>(configuration.GetSection("Storage"));
        services.Configure<ReceiptAnalyzerOptions>(configuration.GetSection("ReceiptAnalyzer"));
        services.Configure<OpenAiOptions>(configuration.GetSection("OpenAi"));
        services.Configure<AiProviderOptions>(configuration.GetSection("AiProvider"));
        services.Configure<RecipeSuggestionOptions>(configuration.GetSection("RecipeSuggestion"));

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IReceiptService, ReceiptService>();
        services.AddScoped<IReceiptAnalyzerService, ReceiptAnalyzerService>();
        services.AddScoped<IRecipeSuggestionService, RecipeSuggestionService>();
        services.AddScoped<IInventoryService, InventoryService>();

        return services;
    }

    public static IServiceCollection AddAiServices(this IServiceCollection services)
    {
        services.AddHttpClient<OpenAiService>();
        services.AddScoped<IAiRecipeServiceFactory, AiRecipeServiceFactory>();

        return services;
    }

    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<UploadReceiptDtoValidator>();

        return services;
    }

    public static IServiceCollection AddAutoMapperProfiles(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(ReceiptProfile).Assembly);

        return services;
    }

    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connString = configuration.GetConnectionString("Default")
                         ?? "Server=(localdb)\\MSSQLLocalDB;Database=FoodAdviser;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        services.AddFoodAdviserInfrastructure(
            configuration,
            options => options.UseNpgsql(connString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null);
                npgsqlOptions.CommandTimeout(60);
            }));

        return services;
    }

    public static IServiceCollection AddAuthenticationAndAuthorization(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddFoodAdviserIdentity(_ =>
        {
            // Customize Identity options here if needed
            // _.Password.RequiredLength = 10;
        });

        services.AddJwtAuthentication(configuration);

        return services;
    }
}
