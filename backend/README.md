# FoodAdviser Backend

FoodAdviser is an ASP.NET Core (.NET 10) application that manages food inventory, analyzes shopping receipts using AI, and suggests recipes based on available ingredients and portions. The application includes JWT-based authentication and integrates with OpenAI for intelligent recipe suggestions.

## Contributing Guidance

Please review our development guidelines before opening pull requests or generating code with AI assistants:

- **Copilot Instructions**: See `.github/copilot-instructions.md` for architectural requirements
- **Backend Guidelines**: See `.github/instructions/backend.instructions.md` for detailed backend development rules (Controllers, DI, Repository Pattern, DTOs, XML comments, validation, etc.)

Following these guidelines helps keep the codebase consistent and maintainable.

## Getting Started

- Solution file: `FoodAdviser.slnx`
- IDE: JetBrains Rider or Visual Studio 2022+
- SDK: .NET 10

Clone the repository and open the solution file to begin.

## Project Layout

### Src/FoodAdviser.Api
- **Controllers**: AuthController, InventoryController, ReceiptsController, RecipesController
- **DTOs**: Request/response models for all API endpoints
- **Extensions**: Service registration, options validation, application configuration
- **Validators**: FluentValidation validators for requests
- **Mapping**: AutoMapper profiles for API-level mappings

### Src/FoodAdviser.Application
- **Services**: InventoryService, ReceiptService, RecipeSuggestionService
- **DTOs**: Application-layer data transfer objects
- **Options**: Configuration classes (AiProviderOptions, JwtOptions, OpenAiOptions, ReceiptAnalyzerOptions, RecipeSuggestionOptions, StorageOptions)
- **Mapping**: AutoMapper profiles for domain↔DTO conversions

### Src/FoodAdviser.Domain
- **Entities**: ApplicationUser, ApplicationRole, FoodItem, Receipt, Recipe, RefreshToken
- **Repositories**: Repository interfaces (IFoodItemRepository, IReceiptRepository, IRecipeRepository, IRefreshTokenRepository)
- **Enums**: Domain enumerations

### Src/FoodAdviser.Infrastructure
- **Persistence**: EF Core DbContext and migrations
- **Repositories**: Repository implementations
- **Services**: AuthService, JwtTokenService, CurrentUserService, OpenAiService, ReceiptAnalyzerService, AiRecipeServiceFactory
- **DependencyInjection**: Infrastructure service registration

### Tests
- **FoodAdviser.Api.Tests**: Integration tests for controllers and services
- **FoodAdviser.Infrastructure.Tests**: Unit tests for infrastructure services

## Run (Dev)

### Prerequisites: PostgreSQL Database

The application requires PostgreSQL to run. You have two options:

#### Option 1: Docker Compose (Recommended)

If you have Docker Desktop installed:

```powershell
cd "C:\AI training\FoodAdviser\backend"
docker-compose up -d
```

This will start PostgreSQL on `localhost:5432` with the credentials configured in `appsettings.Development.json`.

To stop PostgreSQL:
```powershell
docker-compose down
```

#### Option 2: Local PostgreSQL Installation

Install PostgreSQL 15+ locally and create a database with these credentials:
- Host: localhost
- Port: 5432
- Database: foodadviser
### Authentication

The API uses JWT Bearer token authentication. All endpoints except `/api/Auth/register` and `/api/Auth/login` require authentication.

To authenticate:
1. Register or login to receive access and refresh tokens
2. Include the access token in the `Authorization` header: `Bearer {token}`
3. Use the refresh token to get new access tokens when they expire

### Endpoints

#### Authentication (`/api/Auth`)
- `POST /api/Auth/register` - Register a new user (anonymous)
- `POST /api/Auth/login` - Login and receive tokens (anonymous)
- `POST /api/Auth/refresh` - Refresh access token (anonymous)
- `POST /api/Auth/logout` - Logout and invalidate refresh token (authenticated)

#### Inventory (`/api/Inventory`)
- `GET /api/Inventory?page=1&pageSize=20` - Get paginated food items
- `GET /api/Inventory/{id}` - Get a specific food item
- `POST /api/Inventory` - Add a new food item
- `PUT /api/Inventory/{id}` - Update a food item
- `DELETE /api/Inventory/{id}` - Delete a food item

#### Receipts (`/api/Receipts`)
- `POST /api/Receipts/upload` - Upload and analyze a receipt image
- `GET /api/Receipts/recent` - Get recent receipts

#### Recipes (`/api/Recipes`)
- `POST /api/Recipes/generate` - Generate recipe suggestions based on available inventory
- `POST /api/Recipes/confirm` - Confirm a recipe and deduct ingredients from inventory

- `https://localhost:7162/swagger`

All API endpoints are under the `/api` prefix.

Endpoints:

- Inventory
  - `GET /api/Inventory?page=1&pageSize=20`
  - `GET /api/Inventory/{id}`
  - `POST /api/Inventory`
  - `PUT /api/Inventory/{id}`
  - `DELETE /api/Inventory/{id}`
Configuration is managed through `appsettings.json` and `appsettings.Development.json`. All configuration sections are validated at startup.

### Database Connection
- **Development**: Uses PostgreSQL (see connection string in `appsettings.Development.json`)
- **Production**: Configure `ConnectionStrings:Default` in `appsettings.json` or environment variables

Connection String Parameters:
- Timeout: 30 seconds
- CommandTimeout: 30 seconds
- MaxPoolSize: 100
- Retry on failure: 5 attempts with exponential backoff (max 10 seconds delay)

### JWT Authentication (`Jwt` section)
```json
{
  "SecretKey": "YourSecretKey",
  "Issuer": "FoodAdviser.Api",
  "Audience": "FoodAdviser.Client",
  "ExpirationMinutes": 60,
  "RefreshTokenExpirationDays": 7
}
```

### OpenAI Integration (`OpenAi` section)
```json
{
  "ApiKey": "your-openai-api-key",
  "Model": "gpt-4",
  "TimeoutSeconds": 600
}
```
Used for generating recipe suggestions based on available ingredients.

### Receipt Analyzer (`ReceiptAnalyzer` section)
```json
{
  "ClientId": "your-client-id",
  "Username": "your-username",
  "ApiKey": "your-api-key",
  "TimeoutSeconds": 30,
  "RetryCount": 3,
  "RetryDelayMs": 500
}
```
Integrates with external receipt OCR service to extract product information from uploaded receipts.

### Storage (`Storage` section)
```json
{
  "ReceiptTempPath": "C:\\Temp\\FoodAdviser\\Receipts-Dev"
}
```
Local directory for temporary receipt file storage. The application automatically creates this directory on startup.
- **Development**: Uses PostgreSQL (see connection string in `appsettings.Development.json`)
- **Production**: Configure `ConnectionStrings:Default` in `appsettings.json` or environment variables

### Connection String Parameters
The application is configured with:
- Timeout: 30 seconds
- CommandTimeout: 30 seconds
- Retry on failure: 5 attempts with exponential backoff (max 10 seconds delay)

### Other Settings
- AutoMapper registered via Application profile
- Receipt analyzer configured via `ReceiptAnalyzer` section
- OpenAI integration via `OpenAi` section
