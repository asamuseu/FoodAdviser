using FoodAdviser.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure application services
builder.Services
    .AddCorsPolicy()
    .AddAutoMapperProfiles()
    .AddApplicationOptions(builder.Configuration)
    .AddApplicationServices()
    .AddAiServices()
    .AddValidation()
    .AddDatabase(builder.Configuration)
    .AddAuthenticationAndAuthorization(builder.Configuration);

var app = builder.Build();

// Validate all configuration options at startup
app.Services.ValidateAllOptions();

// Initialize application
await app.ApplyDatabaseMigrationsAsync();
await app.EnsureStorageDirectoryAsync();

// Configure the HTTP request pipeline
app.ConfigureMiddleware();
app.MapControllers();

app.Run();
