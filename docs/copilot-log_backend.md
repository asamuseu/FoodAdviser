This project is a FoodManager application built with ASP.NET Core (.NET 10).The application manages food inventory, analyzes shopping receipts, and suggests recipes based on available ingredients and portions.
Create an ASP.NET Core Web API project structure using .NET 10.
Use clean architecture with layers: API, Application, Domain, Infrastructure.Add basic folders for Controllers, Services, Repositories, DTOs, and Entities.


Design domain entities for a food inventory system.
Include Product, InventoryItem (product + quantity), Recipe, RecipeIngredient, and DishType enum (Salad, Soup, MainCourse, Dessert, Appetizer).Use C# records or classes suitable for EF Core.

Configure Entity Framework Core with PostgreSQL for this project.
Create DbContext and DbSet for Products, InventoryItems, Recipes, and RecipeIngredients.

Create API endpoints to add, update, and delete products in user inventory. Use RESTful conventions and DTOs. Implement validation to prevent negative product quantities.

Create an API endpoint to upload a receipt image and extract product names and quantities from it.
Assume OCR result is a plain text list of items for now. Add a service that parses receipt text and maps items to existing products in inventory.

Create a DishType enum with values: Salad, Soup, MainCourse, Dessert, Appetizer.

Add a new API endpoint and implement all required logic for it.
The endpoint should accept:
- dish type
- number of persons
If the input data is valid, start a process that:
Retrieves all available products from the database for the current user, including only items with quantity greater than 0.
Uses this list of products and their quantities to call OpenAI and request N recipe suggestions for the selected dish type and number of persons.
The value of N must be read from configuration (appsettings.json).
Each recipe returned by OpenAI must include:
- recipe name
- list of required products with quantities
- recipe description
The OpenAI response must be returned in JSON format, suitable for deserialization into an array of Recipe objects.
Deserialize the response, save the generated recipes to the database, and return them as the endpoint response.
If it is not possible to generate any recipes using the available products, return a clear message indicating that no suitable recipes were found.

Add a new API endpoint and implement all required logic for it.
The endpoint should accept a collection of recipe IDs.
Based on the provided recipe IDs:
Retrieve the corresponding recipes from the database for the current user.
For each recipe, get the list of ingredients (product names) and the quantities specified in the recipe.
Update the user inventory by subtracting the used product quantities according to the recipe data.
Persist all inventory changes to the database.
Example:
Recipe uses 2 tomatoes and 1 onion
User inventory contains 5 tomatoes and 3 onions
After confirmation, inventory should be updated to 3 tomatoes and 2 onions
Ensure that inventory updates are performed consistently and saved to the database.

Add an initial migration and a startup-time Database.Migrate() so the schema is created automatically when the API starts

Add ASP.NET Core Identity to the FoodAdviser API.
Configure Identity with Entity Framework Core and PostgreSQL.
Use a custom ApplicationUser entity with a GUID as the primary key.

Configure JWT authentication for the FoodAdviser API.
Generate JWT tokens on successful login and include the user ID as a claim. Configure token validation parameters.

Check  the API endpoint for user login whether it
validates user credentials using ASP.NET Identity and returns a JWT access token on success.

Update all business logic to retrieve the current user ID from JWT claims.Ensure all user-specific data is scoped to the authenticated user.

Implement JWT access token refresh functionality in the FoodAdviser API.
Requirements:
- The API uses JWT bearer authentication
- Access tokens are short-lived
- Refresh tokens are long-lived and securely stored
Security requirements:
- Reject expired or revoked refresh tokens
- Handle invalid token scenarios gracefully
- Do not expose sensitive data in responses
- Use proper HTTP status codes
Constraints:
- Use existing authentication setup
- Keep changes backward-compatible
- Follow production-ready best practices

---Refactoring

Remove Mappings from the controller to dedicated file

Move all interfeces from the Services folder to 'Services/Interfaces'

Create a universal configuration validator for a .NET 10 Web API project.
Context:
I have multiple configuration sections in appsettings.json (ConnectionStrings, Jwt, ReceiptAnalyzer, Storage, OpenAi, etc.).
Some sensitive values are injected at runtime from a secrets manager.
In appsettings they are temporarily stored as the placeholder:
{injected-from-secrets-manager}
Task:
Implement a universal validation mechanism that will:
Scan all configuration sections after binding to strongly typed settings classes.
Detect any property whose value equals the placeholder string:{injected-from-secrets-manager}
If at least one such value is found — throw an exception at application startup.
The exception message must clearly indicate:
- Which configuration section failed
- Which property is invalid
- That the secret was not injected
Example message:
"Configuration validation failed: Secret value for 'Jwt.SecretKey' was not injected from Secrets Manager."
Technical requirements:
- Use Options pattern (IOptions / IOptionsMonitor).
- Work with multiple settings classes automatically (not hard-coded per class).
- Support nested objects if present.
- Run validation during application startup (Program.cs).
- Follow best practices for .NET configuration validation.
- Make the placeholder string a constant.
- Ensure the validator is easily extendable for future rules.
Deliverables:
- Validator implementation class.
- Example of registering and using it in Program.cs.
- Example settings classes if needed for demonstration.
Code must be production-ready and cleanly structured.

Add unit tests for controllers and services. Use xunit and if it is needed AutoFixture, NSubstitute. Follow best practices for testing.

#file:backend Create a multi-stage Dockerfile at backend/Dockerfile for a .NET 10.0 API application with Clean Architecture. Use SDK image for build, ASP.NET runtime for final image, expose port 8080.

-- GitHub Actions Workflow Prompt --
Create a GitHub Actions workflow file at `.github/workflows/backend-ci.yml` for a backend project using .NET.
## Triggers
Run the workflow on:
* Push to branches:
  * `main`
  * `develop`
  * `feature/**`
* Pull Requests targeting `main`
Use **path filters** so the workflow runs only when:
* Files inside `backend/**` change, OR
* The workflow file itself changes
---
## General Requirements
* Follow GitHub Actions and .NET CI best practices
* Use the latest stable runner
* Use environment variables where appropriate
* Use concurrency control to cancel outdated runs on the same branch/PR
* Use minimal permissions:
---
## Jobs Structure
Split the workflow into multiple jobs with clear responsibilities and dependencies.
---
### 1️⃣ restore
Steps:
* Checkout repository (`actions/checkout`)
* Setup .NET SDK:
  * Use latest LTS
  * Cache NuGet packages using `actions/cache`
  * Cache path: `~/.nuget/packages`
---
### 2️⃣ lint-and-format
**Depends on:** `restore`
Steps:
* Checkout repo
* Setup .NET
* Restore (using cache)
* Run formatting check:
* Formatting issues must fail the job
---
### 3️⃣ build
**Depends on:** `lint-and-format`
Steps:
* Checkout repo
* Setup .NET
* Restore (cached)
* Build solution:
Requirements:
* Treat warnings as errors
* Do not run tests here
---
### 4️⃣ test
**Depends on:** `build`
Steps:
* Checkout repo
* Setup .NET
* Restore (cached)
Run tests with coverage enabled using Coverlet (built-in collector):
Requirements:
* Fail job if any test fails
* Generate coverage files in Cobertura format
---
### 5️⃣ coverage-report
**Depends on:** `test`
Steps:
1. Install ReportGenerator tool:
2. Generate coverage report:
3. Upload coverage report as workflow artifact:
---
## Expected Output
Copilot must generate:
* A complete production-ready YAML workflow
* Proper job dependencies using `needs`
* Optimized NuGet caching
* Test execution with coverage collection
* Coverage report generation
* Artifact upload
Create a GitHub Actions workflow file at `.github/workflows/backend-ci.yml` for a backend project using .NET.
## Triggers
Run the workflow on:
* Push to branches:
  * `main`
  * `develop`
  * `feature/**`
* Pull Requests targeting `main`
Use **path filters** so the workflow runs only when:
* Files inside `backend/**` change, OR
* The workflow file itself changes
---
## General Requirements
* Follow GitHub Actions and .NET CI best practices
* Use the latest stable runner
* Use environment variables where appropriate
* Use concurrency control to cancel outdated runs on the same branch/PR
* Use minimal permissions:
---
## Jobs Structure
Split the workflow into multiple jobs with clear responsibilities and dependencies.
---
### 1️⃣ restore
Steps:
* Checkout repository (`actions/checkout`)
* Setup .NET SDK:
  * Use latest LTS
  * Cache NuGet packages using `actions/cache`
  * Cache path: `~/.nuget/packages`
---
### 2️⃣ lint-and-format
**Depends on:** `restore`
Steps:
* Checkout repo
* Setup .NET
* Restore (using cache)
* Run formatting check:
* Formatting issues must fail the job
---
### 3️⃣ build
**Depends on:** `lint-and-format`
Steps:
* Checkout repo
* Setup .NET
* Restore (cached)
* Build solution:
Requirements:
* Treat warnings as errors
* Do not run tests here
---
### 4️⃣ test
**Depends on:** `build`
Steps:
* Checkout repo
* Setup .NET
* Restore (cached)
Run tests with coverage enabled using Coverlet (built-in collector):
Requirements:
* Fail job if any test fails
* Generate coverage files in Cobertura format
---
### 5️⃣ coverage-report
**Depends on:** `test`
Steps:
1. Install ReportGenerator tool:
2. Generate coverage report:
3. Upload coverage report as workflow artifact:
---
## Expected Output
Copilot must generate:
* A complete production-ready YAML workflow
* Proper job dependencies using `needs`
* Optimized NuGet caching
* Test execution with coverage collection
* Coverage report generation
* Artifact upload
