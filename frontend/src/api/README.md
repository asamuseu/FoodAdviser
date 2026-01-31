## API
Base URLs
- HTTP: `http://localhost:5288`
- HTTPS: `https://localhost:7162`

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
