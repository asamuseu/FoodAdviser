# Copilot Instructions for FoodAdviser (ASP.NET Core, .NET 10)
---
applyTo: "backend/**"
---

Purpose: Ensure generated code aligns with the project's architectural and quality standards for a FoodAdviser application that manages food inventory, analyzes shopping receipts, and suggests recipes based on available ingredients and portions.

Core Guidelines

- Use Controllers for API endpoints
  - Implement API endpoints using ASP.NET Core MVC controllers (attribute routing).
  - Group endpoints by bounded context (e.g., InventoryController, ReceiptsController, RecipesController).
  - Avoid minimal APIs for production endpoints unless explicitly requested.

- Use Dependency Injection (DI)
  - Register services, repositories, and configuration via the built-in ASP.NET Core DI container.
  - Prefer constructor injection; avoid service locator patterns.
  - Keep service lifetimes appropriate: Singleton for stateless, idempotent cross-app services; Scoped for per-request services/repositories; Transient for lightweight, stateless utilities.

- Use Repository Pattern
  - Abstract data access behind repository interfaces (e.g., IFoodItemRepository, IReceiptRepository, IRecipeRepository).
  - Implement repositories with clear responsibilities and avoid business logic leakage.
  - Use UoW or transactional patterns when multiple repository operations must be committed atomically.

- Follow Best Practices
  - SOLID principles, clean architecture boundaries, and separation of concerns.
  - Validation: Use FluentValidation; validate at the controller boundary.
  - Error handling: Centralize with middleware; return ProblemDetails for consistent API errors.
  - Logging: Use Microsoft.Extensions.Logging with structured logs and correlation where relevant.
  - Configuration: Use options pattern (IOptions<T>) for config sections.
  - Async: Use async/await consistently for I/O bound operations; avoid synchronous blocking.
  - Security: Apply authentication/authorization attributes at controllers/actions where needed; validate and sanitize external inputs.
  - Mapping: Use AutoMapper or explicit mapping functions for DTO↔domain conversions.
  - Persistence: Prefer EF Core with migrations; avoid leaking EF types outside repositories/services.
  - Testing: Add unit tests for services/repositories and integration tests for controllers.

- Use DTOs Where It Makes Sense
  - Expose DTOs for request/response payloads; do not expose domain entities directly.
  - Keep DTOs focused on API contracts; include validation attributes or validators.
  - Provide separate DTOs for create/update/read when fields differ.

- Add XML Summary Comments
  - Add XML doc comments (/// <summary>…</summary>) for all controller classes and action methods.
  - Add XML doc comments for DTOs and public properties.
  - Include remarks for edge cases and usage notes when relevant.

- Use Individual Files
  - Place each class, interface, repository, service, DTO, validator, and controller in its own file.
  - Organize by feature folders or conventional folders (Controllers, Domain, Infrastructure, Application, DTOs, etc.).

- Always Show the Plan First
  - Before generating code, output a concise plan summarizing:
    - Affected files and their locations
    - Interfaces/classes to add or modify
    - Endpoints and DTOs to implement
    - Tests and any configuration changes

Conventions

- Naming
  - Controllers end with “Controller” (e.g., InventoryController).
  - Interfaces start with “I” (e.g., IFoodItemRepository, IReceiptAnalyzerService).
  - DTOs end with “Dto” (e.g., CreateFoodItemDto, FoodItemDto).

- API
  - Use attribute routing: [Route("api/[controller]")]; [HttpGet], [HttpPost], [HttpPut], [HttpDelete].
  - Return IActionResult or ActionResult<T>; use appropriate status codes.
  - Paginate list endpoints and support filtering/sorting when practical.

- Validation & Error Responses
  - Return 400 for validation failures with detailed messages.
  - Use ProblemDetails for standardized error payloads.

- Documentation & Comments
  - Keep XML summaries accurate and updated.
  - Add high-level comments where complex logic exists.

- Performance & Resilience
  - Consider caching for frequent reads.
  - Use cancellation tokens for async operations where applicable.

- Data & Transactions
  - Encapsulate transaction boundaries in application services/UoW.
  - Avoid leaking DbContext beyond repositories.

- Mapping
  - Centralize mapping profiles; avoid inline ad-hoc mapping scattered across controllers.

- Testing
  - Controllers: integration-style tests using TestServer/WebApplicationFactory.
  - Services/Repositories: unit tests with mocks (e.g., Moq) where appropriate.

- Code Style
  - Follow .editorconfig or project style; prefer explicit accessibility modifiers, nullable reference types enabled, and meaningful names.

Notes for Copilot

- Prefer generating full, compilable fragments and remember to add necessary usings.
- Suggest lightweight scaffolding (interfaces, DTOs, controllers) aligned with these rules.
- Avoid adding unrelated dependencies unless required; prefer built-in frameworks.
- If ambiguity exists, propose 1–2 reasonable assumptions and continue.
