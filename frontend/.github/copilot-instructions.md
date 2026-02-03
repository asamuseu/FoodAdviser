# Copilot Instructions — FoodAdviser Frontend (React + Vite + TypeScript)

Base context
- React application (Vite + TypeScript) for the FoodAdviser frontend.
- Backend is an ASP.NET Core Web API (.NET 10) located in `backend/`.
- API contract is defined in `docs/openapi.json`.

Primary goals
- Keep the frontend focused on UI and orchestration; business rules belong in the backend.
- Prefer simple, explicit code with strong TypeScript types.

Repository conventions
- Put API-related code in `frontend/src/api/`.
- Presentational components should not embed raw `fetch` calls; use an API client module.
- Keep one React component per file.

API usage
- Do not hard-code API base URLs inside components.
- Use a Vite env var for the API base URL (example: `VITE_API_BASE_URL`).
- Local dev base URLs (from `frontend/src/api/README.md`):
  - HTTP: `http://localhost:5288`
  - HTTPS: `https://localhost:7162`

API client expectations
- Centralize request logic in a small client wrapper that:
  - Prefixes routes with the configured base URL.
  - Sets headers consistently (e.g. `Content-Type: application/json` when applicable).
  - Parses JSON responses safely.
  - Normalizes errors (status + message/details) into a predictable shape.
  - Supports cancellation via `AbortController`.

OpenAPI alignment
- When creating request/response types or adding new API calls, align them with `docs/openapi.json`.
- If anything is ambiguous, consult `docs/openapi.json` before guessing.

Known endpoints (high-level)
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

React + TypeScript guidance
- Use function components and hooks.
- Avoid `any`; prefer explicit types for props, state, and API payloads.
- Prefer derived state over duplicated state.
- Handle all async states: loading, error (user-visible), success.
- Avoid race conditions by canceling in-flight requests on unmount/param change.

Forms & validation
- Validate obvious UI constraints (required fields, basic formatting).
- Treat backend validation as authoritative; surface API validation errors clearly.

When generating code
- Keep changes minimal and consistent with existing structure.
- If build tooling files are absent in `frontend/` (e.g. `package.json`, `vite.config.*`), do not invent scripts or scaffold a new toolchain unless explicitly requested.

Use Individual Files
  - Place each class, interface, service, DTO in its own file.
  - Organize by feature folders or conventional folders (pages,dtos, etc.).

We want to redesign the FoodAdviser frontend UI.

Design goals:
 - Modern, clean, minimalistic UI
 - Friendly and calm look suitable for a food / kitchen assistant app
 - Focus on readability and simplicity
 - Mobile-first, responsive layout

Visual style:
 - Soft rounded components
 - Subtle shadows
 - Spacious layout with clear visual hierarchy

Do not create comprehensive summary file afeter changes.
