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
```
cd "C:\AI training\FoodAdviser\FoodAdviser.Api"
dotnet run
```

## Configuration
- ConnectionStrings:Default uses SQLite file `foodadviser.db`.
- AutoMapper registered via Application profile.
