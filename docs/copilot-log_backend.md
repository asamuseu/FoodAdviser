Add an initial migration and a startup-time Database.Migrate() so the schema is created automatically when the API starts

Add ASP.NET Core Identity to the FoodAdviser API.
Configure Identity with Entity Framework Core and PostgreSQL.
Use a custom ApplicationUser entity with a GUID as the primary key.

Configure JWT authentication for the FoodAdviser API.
Generate JWT tokens on successful login and include the user ID as a claim. Configure token validation parameters.

Check  the API endpoint for user login whether it
validates user credentials using ASP.NET Identity and returns a JWT access token on success.

Update all business logic to retrieve the current user ID from JWT claims.Ensure all user-specific data is scoped to the authenticated user.