# FoodAdviser

FoodAdviser is an ASP.NET Core (.NET 10) application that manages food inventory, analyzes shopping receipts, and suggests recipes based on available ingredients and portions.

## Contributing Guidance

Please review our development guidelines before opening pull requests or generating code with AI assistants:

- Copilot Instructions: See `.github/copilot-instructions.md` for architectural requirements (Controllers for API endpoints, Dependency Injection, Repository Pattern, DTOs, XML summary comments, one-class-per-file organization, and always show a plan first).

Following these guidelines helps keep the codebase consistent and maintainable.

## Getting Started

- Solution file: `FoodAdviser.slnx`
- IDE: JetBrains Rider or Visual Studio 2022+
- SDK: .NET 10

Clone the repository and open the solution file to begin.

## Project Layout
- FoodAdviser.Api: Controllers, middleware, DI composition
- FoodAdviser.Application: DTOs, mappings, validators
- FoodAdviser.Domain: Entities, repository/service interfaces
- FoodAdviser.Infrastructure: EF Core DbContext, repositories, DI
- FoodAdviser.Api.Tests: Integration tests (to be expanded)

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
- Username: foodadviser
- Password: foodadviser_dev_pw

### Running the Application

```powershell
cd "C:\AI training\FoodAdviser\backend\FoodAdviser.Api"
dotnet run
```

## API

Base URLs (Development defaults from `FoodAdviser.Api/Properties/launchSettings.json`):

- HTTP: `http://localhost:5288`
- HTTPS: `https://localhost:7162`

Swagger UI (Development only):

- `https://localhost:7162/swagger`

All API endpoints are under the `/api` prefix.

Endpoints:

- Inventory
  - `GET /api/Inventory?page=1&pageSize=20`
  - `GET /api/Inventory/{id}`
  - `POST /api/Inventory`
  - `PUT /api/Inventory/{id}`
  - `DELETE /api/Inventory/{id}`

- Receipts
  - `POST /api/Receipts/upload`
  - `GET /api/Receipts/recent`
  - `POST /api/Receipts/analyze` (stub)

- Recipes
  - `POST /api/Recipes/generate`
  - `POST /api/Recipes/confirm`
  - `GET /api/Recipes/suggestions?max=10` (stub)

## Configuration

### Database Connection
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
