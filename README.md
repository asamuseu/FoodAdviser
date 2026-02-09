# FoodAdviser

**FoodAdviser** is a comprehensive food inventory management and recipe suggestion application that helps users track their available ingredients, scan shopping receipts, and generate personalized recipes based on what they have at home.

---

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Development Journey](#development-journey)
- [AI Tools & Models Used](#ai-tools--models-used)
- [Insights & Recommendations](#insights--recommendations)
- [License](#license)

---

## 🎯 Overview

FoodAdviser simplifies kitchen management by:
- **Managing Food Inventory**: Track products and quantities available in your kitchen
- **Receipt Scanning**: Upload shopping receipts to automatically update inventory
- **Smart Recipe Suggestions**: Generate personalized recipes based on available ingredients and servings
- **User Authentication**: Secure, multi-user support with JWT-based authentication

The application follows clean architecture principles and modern best practices for both backend and frontend development.

---

## ✨ Features

### Core Functionality
1. **Inventory Management**
   - Add, update, and remove products manually
   - Track quantities of available ingredients
   - Real-time inventory updates

2. **Receipt Analysis**
   - Upload receipt images
   - Automatic OCR and product extraction
   - Automatic inventory updates from receipts

3. **Recipe Generation**
   - AI-powered recipe suggestions using OpenAI
   - Filter by dish categories: Salad, Soup, Main Course, Dessert, Appetizer
   - Specify number of servings/persons
   - Ingredient requirements with quantities
   - Recipe confirmation and automatic inventory deduction

4. **Authentication & Security**
   - User registration and login
   - JWT access and refresh tokens
   - Protected routes and API endpoints
   - Automatic token refresh handling

---

## 🛠️ Technology Stack

### Backend
- **Framework**: ASP.NET Core (.NET 10)
- **Architecture**: Clean Architecture (API, Application, Domain, Infrastructure)
- **Database**: PostgreSQL with Entity Framework Core
- **Authentication**: ASP.NET Core Identity + JWT
- **API Documentation**: OpenAPI/Swagger
- **Testing**: xUnit, NSubstitute, AutoFixture
- **AI Integration**: OpenAI for recipe generation

### Frontend
- **Framework**: React 18
- **Build Tool**: Vite
- **Language**: TypeScript
- **Package Manager**: pnpm
- **Styling**: Custom CSS with modern design system
- **State Management**: React Context (Auth)
- **API Client**: Centralized HTTP client with automatic token refresh

### DevOps
- **Containerization**: Docker (multi-stage builds)
- **CI/CD**: GitHub Actions
  - Backend: restore → lint (non-blocking) → build → test → coverage-report + docker (main/develop only)
  - Frontend: install → build → publish-artifact + docker-build-and-push
- **Container Registry**: GitHub Container Registry (GHCR)

---

## 📁 Project Structure

```
FoodAdviser/
├── backend/                    # ASP.NET Core Web API
│   ├── Src/
│   │   ├── FoodAdviser.Api/          # API controllers, DTOs, validators
│   │   ├── FoodAdviser.Application/  # Services, application logic
│   │   ├── FoodAdviser.Domain/       # Entities, repositories, enums
│   │   └── FoodAdviser.Infrastructure/ # EF Core, external services
│   ├── Tests/
│   │   ├── FoodAdviser.Api.Tests/
│   │   └── FoodAdviser.Infrastructure.Tests/
│   ├── docker-compose.yml
│   └── Dockerfile
│
├── frontend/                   # React + Vite + TypeScript
│   ├── src/
│   │   ├── api/              # API client, models, schema
│   │   ├── components/       # Reusable UI components
│   │   ├── contexts/         # React contexts (AuthContext)
│   │   ├── pages/            # Application pages
│   │   ├── styles/           # Modular CSS
│   │   └── utils/            # Utility functions
│   ├── public/               # Static assets
│   ├── package.json
│   ├── vite.config.ts
│   └── Dockerfile
│
├── docs/                       # Documentation
│   └── propts-logs/           # AI prompt history
│
├── .github/
│   ├── copilot-instructions.md # Copilot configuration
│   ├── instructions/           # Backend & frontend instructions
│   └── workflows/              # CI/CD pipelines
│
└── README.md                   # This file
```

---

## 🚀 Getting Started locally

### Prerequisites
- .NET 10 SDK
- Node.js 18+ and pnpm
- PostgreSQL 14+
- Docker (optional, for containerized deployment)

### Backend Setup

1. Navigate to backend directory:
   ```bash
   cd backend
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. **Configure application settings:**
   
   Open `Src/FoodAdviser.Api/appsettings.json` and replace all placeholder values `{injected-from-secrets-manager}` with actual configuration values:
   
   - **ConnectionStrings:Default** - Your PostgreSQL connection string (e.g., `Host=localhost;Database=foodadviser;Username=postgres;Password=yourpassword`)
   - **Jwt:SecretKey** - A secure random string for JWT signing (minimum 32 characters)
   - **Jwt:Issuer** - Your API issuer (default: `FoodAdviser.Api`)
   - **Jwt:Audience** - Your API audience (default: `FoodAdviser.Client`)
   - **OpenAi:ApiKey** - Your OpenAI API key for recipe generation
   - **ReceiptAnalyzer:ClientId** - Receipt analyzer service client ID (if using receipt scanning)
   - **ReceiptAnalyzer:Username** - Receipt analyzer service username
   - **ReceiptAnalyzer:ApiKey** - Receipt analyzer service API key
   - **Storage:ReceiptTempPath** - Temporary path for receipt uploads (e.g., `./temp/receipts`)
   
   > ⚠️ **Important**: Never commit real secrets to version control. Use environment variables, user secrets, or a secrets manager in production.

4. Run migrations:
   ```bash
   dotnet ef database update
   ```

5. Start the API:
   ```bash
   dotnet run --project Src/FoodAdviser.Api
   ```

API will be available at:
- HTTP: `http://localhost:5288`
- HTTPS: `https://localhost:7162`

### Frontend Setup

1. Navigate to frontend directory:
   ```bash
   cd frontend
   ```

2. Install dependencies:
   ```bash
   pnpm install
   ```

3. Configure environment variables:
   ```bash
   cp .env.example .env.local
   ```

4. Start development server:
   ```bash
   pnpm dev
   ```

Frontend will be available at `http://localhost:5173`

### Docker Setup

Build and run with Docker Compose:
```bash
docker-compose up --build
```

## 📝 Development Journey

The development process and prompt history are documented for transparency and learning purposes:
- [Backend Development Log](docs/propts-logs/copilot-log_backend.md)
- [Frontend Development Log](docs/propts-logs/copilot-log-frontend.md)

### Phase 1: Backend Foundation
**Key Steps:**
1. ✅ Project structure setup with Clean Architecture
2. ✅ Domain entities design (Product, InventoryItem, Recipe, RecipeIngredient, DishType)
3. ✅ EF Core configuration with PostgreSQL
4. ✅ CRUD endpoints for inventory management
5. ✅ Receipt upload and OCR text parsing
6. ✅ Recipe generation with OpenAI integration
7. ✅ Recipe confirmation and inventory deduction logic
8. ✅ Database migrations and auto-migration on startup

### Phase 2: Authentication & Security
**Key Steps:**
1. ✅ ASP.NET Core Identity integration with custom ApplicationUser
2. ✅ JWT token generation and validation
3. ✅ Refresh token implementation
4. ✅ User-scoped data access (all operations filtered by authenticated user)
5. ✅ Protected endpoints with role-based authorization

### Phase 3: Backend Refactoring & Quality
**Key Steps:**
1. ✅ Mapping logic extracted to dedicated files
2. ✅ Service interfaces moved to `Services/Interfaces`
3. ✅ Universal configuration validator for secrets management
4. ✅ Unit tests for controllers and services (xUnit, NSubstitute, AutoFixture)
5. ✅ Docker multi-stage build implementation
6. ✅ GitHub Actions CI pipeline with coverage reporting

### Phase 4: Frontend Foundation
**Key Steps:**
1. ✅ React + Vite + TypeScript project setup
2. ✅ TypeScript models generated from backend DTOs
3. ✅ Centralized API client with error handling
4. ✅ Inventory page with product listing
5. ✅ Receipt upload page with image preview and loading states
6. ✅ Recipe generation page with dish type selection
7. ✅ Recipe confirmation workflow

### Phase 5: Frontend Authentication
**Key Steps:**
1. ✅ AuthContext for global authentication state
2. ✅ Login page with email/password validation
3. ✅ Registration page
4. ✅ Protected routes implementation
5. ✅ HTTP client integration with JWT Authorization header
6. ✅ Automatic token refresh with retry logic
7. ✅ User-friendly error message parsing

### Phase 6: UI/UX Enhancement
**Key Steps:**
1. ✅ Modern color theme (soft green, warm beige, soft orange)
2. ✅ Modular CSS architecture (base, layout, components, pages, utilities)
3. ✅ Improved buttons, forms, and input controls
4. ✅ Production SaaS landing page design
5. ✅ Responsive mobile-first layout
6. ✅ Subtle UI animations and transitions
7. ✅ Custom favicon
8. ✅ Delete confirmation modals

### Phase 7: DevOps & CI/CD
**Key Steps:**
1. ✅ Backend Dockerfile with multi-stage build
2. ✅ Frontend Dockerfile with pnpm
3. ✅ Backend CI pipeline (restore → lint [non-blocking] → build → test → coverage-report + docker)
4. ✅ Frontend CI pipeline (install → build → publish-artifact + docker-build-and-push)
5. ✅ GitHub Container Registry integration (GHCR)
6. ✅ Path filters for optimized workflow triggers
7. ✅ Docker layer caching (GHA cache) and dependency optimization
8. ✅ Concurrency control to cancel outdated runs

---

## 🤖 AI Tools & Models Used

### Primary Models
- **Claude Sonnet 4.5** (Primary): Used for the majority of code generation, architecture decisions, and complex refactoring tasks
- **GPT-5.2-Codex**: Used for specialized code generation and API contract design
- **Claude Opus 4.5**: Used occasionally for complex architectural decisions and advanced problem-solving

### Development Approach
- **GitHub Copilot**: Integrated throughout the development process
- **Prompt-Driven Development**: All major features documented in `docs/propts-logs/`
- **Iterative Refinement**: Multiple iterations with AI assistance for code quality improvements

### AI-Assisted Areas
1. **Architecture Design**: Clean Architecture setup, layering, dependency injection
2. **Code Generation**: Controllers, services, repositories, DTOs, validators
3. **Testing**: Unit and integration test scaffolding
4. **DevOps**: Dockerfile creation, CI/CD pipeline configuration
5. **Frontend**: Component structure, TypeScript typing, routing, state management
6. **UI/UX**: Design system, responsive layout, accessibility improvements
7. **Documentation**: XML comments, README files, instruction files

---

## 💡 Insights & Recommendations

### What Worked Well

1. **Detailed Copilot Instructions**
   - Custom `.github/copilot-instructions.md` and domain-specific instruction files were crucial
   - Clear architectural constraints prevented drift and maintained consistency
   - Domain-specific rules (backend vs. frontend) kept code aligned with best practices

2. **Incremental Development**
   - Building features step-by-step allowed for validation at each stage
   - Breaking complex tasks into smaller prompts yielded better results
   - Prompt logs served as excellent documentation and learning material

3. **Strong Type Safety**
   - TypeScript on frontend and C# on backend caught many issues early
   - AI models generated more accurate code with explicit types
   - OpenAPI schema alignment kept contracts consistent

4. **Clean Architecture**
   - Separation of concerns made it easier for AI to generate focused code
   - Repository pattern and DI made testing and mocking straightforward
   - Clear boundaries prevented leaky abstractions

5. **Multi-Model Strategy**
   - Different AI models had different strengths
   - Claude Sonnet 4.5 excelled at architectural decisions
   - GPT-5.2-Codex was strong for API contracts and schemas
   - Claude Opus 4.5 provided valuable insights for complex problems

### Recommendations for Future Copilot Usage

#### 1. **Start with Clear Instructions**
   - Create comprehensive `copilot-instructions.md` files early
   - Define architectural patterns, naming conventions, and quality standards upfront
   - Include "do not do" rules to prevent common mistakes
   - Update instructions as the project evolves

#### 2. **Use Prompt Logs**
   - Document all major prompts in a structured way
   - Review prompt effectiveness and refine for better results
   - Share successful prompts across team members
   - Use logs as onboarding material for new developers

#### 3. **Validate AI Output**
   - Always review generated code for correctness
   - Run tests immediately after generation
   - Check for security issues, especially in authentication/authorization
   - Verify performance implications (N+1 queries, memory leaks)

#### 4. **Iterative Refinement**
   - Don't expect perfect code on first generation
   - Use follow-up prompts for refinement ("Make this more testable", "Add error handling")
   - Refactor generated code to match project style
   - Ask for alternatives when initial results aren't optimal

#### 5. **Maintain Human Oversight**
   - AI-generated tests may pass without testing the right behavior
   - Business logic requires human validation
   - Security and privacy concerns need manual review
   - Architecture decisions should involve human judgment

#### 6. **Balance Speed and Quality**
   - AI accelerates development but doesn't replace understanding
   - Take time to understand generated code
   - Refactor when necessary, don't accumulate technical debt
   - Use AI for boilerplate, keep humans for critical decisions

#### 7. **Leverage AI for Different Tasks**
   - **Excellent for**: Boilerplate, DTOs, mapping, basic CRUD, test scaffolding, CI/CD configs
   - **Good for**: Service implementations, validation logic, API clients, UI components
   - **Review carefully**: Security, complex business logic, performance-critical code, architectural decisions

#### 8. **Avoid Common Pitfalls**
   - Don't blindly accept generated code without understanding
   - Watch for over-engineering (AI may suggest unnecessary patterns)
   - Check for missing error handling and edge cases
   - Verify that tests actually test the intended behavior
   - Ensure generated documentation stays up-to-date

#### 9. **Optimize Prompts**
   - Be specific about requirements and constraints
   - Include context (existing code, dependencies, patterns)
   - Ask for explanations when learning new concepts
   - Request alternatives to compare different approaches
   - Specify format (e.g., "one class per file", "XML comments required")

#### 10. **Team Collaboration**
   - Share effective prompts and patterns with the team
   - Establish code review processes for AI-generated code
   - Create team conventions for AI usage
   - Document AI-assisted areas for knowledge transfer
   - Train team members on effective prompt engineering

### Specific to FoodAdviser

1. **Configuration Management**
   - Universal configuration validator proved invaluable
   - Secrets management with placeholder detection prevented runtime issues
   - Options pattern with validation should be default for all settings

2. **Authentication Flow**
   - Automatic token refresh greatly improved UX
   - Proper error parsing made authentication failures user-friendly
   - Refresh token handling prevented unnecessary logouts

3. **Testing Strategy**
   - Unit tests for services/repositories caught integration issues early
   - Integration tests for controllers validated end-to-end behavior
   - Coverage reporting highlighted gaps in testing

4. **CI/CD Pipeline**
   - Separate workflows for backend and frontend allowed parallel development
   - Path filters prevented unnecessary builds
   - Docker layer caching (GitHub Actions cache) significantly reduced build times
   - Artifact publishing enabled easy deployment
   - Concurrency control (cancel-in-progress) optimized resource usage
   - Non-blocking lint job prevented pipeline failures on formatting issues
   - Docker builds only on main/develop branches for backend

5. **Frontend Architecture**
   - Modular CSS organization improved maintainability
   - Centralized API client reduced code duplication
   - Protected routes pattern should be default for authenticated apps

---

## 📄 License

This project is a demonstration/portfolio project developed with AI assistance for educational purposes.

---

## 🙏 Acknowledgments

This project was developed primarily using AI-assisted coding with:
- GitHub Copilot
- Claude Sonnet 4.5 (Anthropic)
- GPT-5.2-Codex (OpenAI)
- Claude Opus 4.5 (Anthropic)

The development process and prompt history are documented for transparency and learning purposes:
- [Backend Development Log](docs/propts-logs/copilot-log_backend.md)
- [Frontend Development Log](docs/propts-logs/copilot-log-frontend.md)

---
