# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution file, project files for dependency restoration (optimizes layer caching)
COPY FoodAdviser.slnx ./
COPY NuGet.config ./
COPY Src/FoodAdviser.Api/FoodAdviser.Api.csproj Src/FoodAdviser.Api/
COPY Src/FoodAdviser.Application/FoodAdviser.Application.csproj Src/FoodAdviser.Application/
COPY Src/FoodAdviser.Domain/FoodAdviser.Domain.csproj Src/FoodAdviser.Domain/
COPY Src/FoodAdviser.Infrastructure/FoodAdviser.Infrastructure.csproj Src/FoodAdviser.Infrastructure/

# Restore dependencies
RUN dotnet restore Src/FoodAdviser.Api/FoodAdviser.Api.csproj

# Copy all source files
COPY Src/ Src/

# Build and publish the application
WORKDIR /src/Src/FoodAdviser.Api
RUN dotnet publish FoodAdviser.Api.csproj -c Release -o /app/publish /p:UseAppHost=false --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN addgroup --system --gid 1001 appgroup && \
    adduser --system --uid 1001 --ingroup appgroup appuser

# Copy published files from build stage
COPY --from=build /app/publish .

# Set ownership to non-root user
RUN chown -R appuser:appgroup /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8080

# Set environment variables for ASP.NET Core
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Start the application
ENTRYPOINT ["dotnet", "FoodAdviser.Api.dll"]
