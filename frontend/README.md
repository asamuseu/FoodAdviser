# FoodManager Frontend (Vite + React + TypeScript)

This frontend consumes the ASP.NET Core Web API described in `docs/openapi.json`.

## Configure

Create `frontend/.env.local`:

```
VITE_API_BASE_URL=https://localhost:7162
```

HTTP alternative:

```
VITE_API_BASE_URL=http://localhost:5288
```

## Generate API types

```
npm run api:gen
```

## Run

```
npm install
npm run dev
```

## Backend (API) local dev

Start the ASP.NET Core API (from repo root):

```powershell
Set-Location "c:\AI training\FoodAdviser"
dotnet run --project .\backend\FoodAdviser.Api\FoodAdviser.Api.csproj
```

Swagger UI (development) is typically at:
- `https://localhost:7162/swagger`

### CORS note

The API currently does not appear to configure CORS in `backend/FoodAdviser.Api/Program.cs`.
If the browser blocks requests from `http://localhost:5173`, you can either:
- Add a CORS policy in the API allowing the Vite dev origin, or
- Add a dev proxy in Vite and call the API through the Vite origin.

### HTTPS dev cert

If you use `https://localhost:7162` and see certificate errors, run:

```powershell
dotnet dev-certs https --trust
```
