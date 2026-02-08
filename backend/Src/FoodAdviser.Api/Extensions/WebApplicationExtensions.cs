using FoodAdviser.Application.Options;
using FoodAdviser.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FoodAdviser.Api.Extensions;

public static class WebApplicationExtensions
{
    public static async Task<WebApplication> ApplyDatabaseMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseMigration");

        try
        {
            var db = scope.ServiceProvider.GetRequiredService<FoodAdviserDbContext>();
            logger.LogInformation("Applying database migrations...");
            await db.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to apply database migrations. Ensure PostgreSQL is running and accessible at the configured connection string.");
            // Optionally rethrow if you want the app to fail fast when DB is unavailable
            // throw;
        }

        return app;
    }

    public static async Task<WebApplication> EnsureStorageDirectoryAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("StorageSetup");
        var storageOptions = scope.ServiceProvider.GetRequiredService<IOptions<StorageOptions>>().Value;

        if (string.IsNullOrWhiteSpace(storageOptions.ReceiptTempPath))
        {
            logger.LogError("StorageOptions.ReceiptTempPath is not configured. Please set Storage:ReceiptTempPath in appsettings.");
            return app;
        }

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

        return app;
    }

    public static WebApplication ConfigureMiddleware(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "FoodAdviser API v1");
            });
        }

        app.UseCors("AllowFrontend");
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}
