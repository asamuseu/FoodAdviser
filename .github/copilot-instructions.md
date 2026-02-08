# FoodAdviser
FoodAdviser is an application that helps users manage food inventory and receive recipe suggestions based on available products.

## 1. General Principles

* Follow existing project architecture and coding style.
* Prefer clean, readable, and maintainable code over complex solutions.
* Do not introduce unnecessary abstractions, libraries, or patterns.
* Generate production-ready code, not demos or prototypes.
* Add comments only where logic is complex or non-obvious.
* Do not duplicate code if reusable services/components already exist.
* If additional information is required — **ask clarifying questions instead of making assumptions**.

---

## 2. Restrictions

* **Do NOT create any `.md` files** (documentation, reports, summaries, analysis, etc.)
  unless explicitly requested by the user.
* Do not generate README files, architecture descriptions, or progress reports unless asked.
* Do not invent requirements or features that were not specified.
* Do not change existing business logic unless requested.
* Do not generate final summary documents unless explicitly requested.

---

## 3. Application Overview — FoodAdviser

FoodAdviser is an application that helps users manage food inventory and receive recipe suggestions based on available products.

### Core Features

1. The application stores a list of products and their quantities available to the user.
2. Users can manually add or remove products and adjust quantities.
3. Products and quantities can also be added or removed automatically by analyzing purchase receipts uploaded by the user (receipt scan processing).
4. The system recognizes 5 dish categories:

   * Salad
   * Soup
   * Main course
   * Dessert
   * Appetizer
5. Upon user request, the system suggests **1–10 recipes** from a selected dish category based on available products and quantities.
6. Each recipe must include the required ingredient quantities.
7. When requesting a recipe, the user must specify the number of servings/persons.
8. If available product types or quantities are insufficient — the system must inform the user that recipe suggestions are not possible.
9. Products are deducted from inventory **only after** the user selects a recipe they will cook.
10. Deduction is based strictly on ingredient quantities defined in the selected recipe.

---

## 4. Technology Stack

### Backend (API)

* ASP.NET Core
* .NET 10
* Entity Framework (EF Core)
* PostgreSQL
* Folder: `backend`

### Frontend (UI)

* React
* Vite
* TypeScript
* Folder: `frontend`

---

## 5. Backend Guidelines

* Use RESTful API conventions.
* Use async/await for database and I/O operations.
* Apply proper layering:

  * Controllers
  * Services
  * Repositories
  * Domain models
* Use EF Core for data access.
* Configure PostgreSQL via connection strings.
* Validate DTOs using FluentValidation or DataAnnotations when applicable.
* Implement proper error handling and HTTP status codes.

---

## 6. Frontend Guidelines

* Use functional React components.
* Use TypeScript with strict typing.
* Prefer hooks over class components.
* Organize code by features/modules where possible.
* Use API service layers for HTTP calls.
* Handle loading, error, and empty states in UI.

---

## 7. Communication Rules for Copilot

* If requirements are unclear — ask questions.
* If data models or contracts are missing — request them.
* If integration details are unknown — clarify before generating code.
* Do not guess database schemas or API contracts without confirmation.

---

## 8. File & Folder Awareness

Respect project structure:

```
/backend   → ASP.NET Core API
/frontend  → React + Vite + TypeScript UI
```

Do not mix backend and frontend code.

---

## 9. Code Quality Expectations

* Use meaningful naming.
* Follow SOLID principles where reasonable.
* Avoid over-engineering.
* Ensure generated code compiles and runs.
* Include minimal unit tests only if explicitly requested.

