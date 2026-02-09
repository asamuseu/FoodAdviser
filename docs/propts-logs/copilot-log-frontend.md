Create a React application (Vite + TypeScript) for FoodManager frontend.
This app consumes an existing ASP.NET Core Web API defined in docs/openapi.json.

Create a file with insructions for GitHub Copilot fronted part of  FoodAdviser application. FoodAdviser API has been written on .Net 10 and stored in the backend folder
 Use this as a base information:
React application (Vite + TypeScript) for FoodAdviser frontend.
This app consumes an existing ASP.NET Core Web API defined in docs/openapi.json.
instruction file : /fronend/.github/copilot-instructions.md

Generate TypeScript interfaces based on the backend DTOs used by the FoodAdviser API.Create for DTOS a dedicated folder and put each dto to a separete file

Create an Inventory page that displays the list of products available in the user inventory.
The page should:
-Load inventory data from the FoodManager API on page load
-Display product name and quantity in a table or list
-Handle loading and empty states (no products available)
-Use TypeScript types based on existing API contracts
-Use the centralized API client from the api folder
The page represents the user’s current food inventory stored in the database.

Create a page for uploading receipt images and displaying extracted products returned by the API. Do not remove receipt image after analyzing. Show a loading spinner (circle) while the receipt is being analyzed/uploaded.

Create a React page that allows the user to generate recipes based on available products.
The page should:
- Allow the user to select a dish type from a predefined list
- Allow the user to enter the number of persons
- Send a request to the FoodManager API to generate recipes
- Display the list of suggested recipes returned by the API
- Each displayed recipe should include:
  recipe name, recipe description, list of ingredients with required quantities
Show a loading spinner (circle) while generation

After successful confirmation:
- Show a success message to the user
- Keep the list of generated recipes visible
- Clear the current recipe selection so the user can select and confirm another recipe

The page should:
- Allow the user to select one generated recipe (e.g. using selection buttons)
- Provide a "Confirm Cooking" action
- When confirmed, send the selected recipe IDs to the FoodAdviser API confirmation endpoint
- Handle success and error responses

Extend the recipe generation page to support recipe confirmation directly on the same page.

------ Add multi-users approsh and Auth
Create an authentication context in the React frontend to manage login state and JWT tokens.
Store the access token securely and expose login and logout functions.

Create a login page with email and password fields.
On successful login, store the JWT token and redirect the user to the main application page.

Create a user registration page with email and password fields.
On successful registration, automatically log in the user or redirect to the login page.

Implement protected routes in the React application.
Only authenticated users should be able to access inventory and recipe pages.

Update the centralized HTTP client to automatically include the JWT access token in the Authorization header for all API requests.

---- UI Redisign ---
Apply a new color theme to the FoodAdviser UI:
- Primary color: soft green (#4CAF50 or similar)
- Secondary color: warm beige / light cream
- Accent color: soft orange for actions and highlights
- Background: light neutral (off-white)
- Text: dark gray, not pure black
- The color palette should feel natural, fresh, and food-related.

Refactor global styles to match the new FoodAdviser design:
- Update base font styles and spacing
- Improve button, input, and card appearance
- Add consistent border-radius and hover states
- Ensure accessibility (contrast, focus states)

Improve action buttons styling:
- Primary actions should stand out clearly
- Secondary actions should be visually lighter
- Add loading and disabled states
- Keep buttons consistent across all pages

Improve forms and input controls:
- Clear labels and placeholders
- Proper spacing between fields
- Validation and error messages styled consistently
- Make forms easy to use on mobile devices

Redesign the recipe generation page:
- Clear separation between input (dish type, number of persons) and results
- Recipes should be easy to scan and compare
- Selection and confirmation actions should be visually clear
- After confirmation, show success feedback without removing the recipes

Add subtle UI animations:
- Smooth transitions for hover and focus
- Loading indicators for async actions
- Avoid heavy or distracting animations

Refactor the existing styles.css file by splitting it into multiple smaller, well-structured CSS files.
Goals:
- Improve maintainability and readability
- Follow production SaaS frontend best practices
- Keep existing styles working without visual regressions
Requirements:
- Do not change class names used in React components
- Do not change visual appearance unless necessary for consistency
- Extract styles into logical groups
Suggested structure:
- base.css (reset, typography, global variables)
- layout.css (page layout, grids, containers)
- components.css (buttons, cards, forms, inputs)
- pages.css (page-specific styles)
- utilities.css (helpers, spacing, text utilities)
Instructions:
- Move existing rules from styles.css into the appropriate new files
- Keep comments explaining each section
- Ensure all new files are properly imported so the UI continues to work

----
Implement automatic access token refresh in the FoodAdviser frontend application.
Requirements:
- The application uses JWT access tokens and refresh tokens
- Access token is sent with API requests in the Authorization header
- When the API responds with 401 Unauthorized due to an expired access token, the app should automatically refresh the token and retry the original request
Implementation details:
- Store access token and refresh token securely on the client
- Add a centralized HTTP layer (e.g. API client or interceptor)
- Intercept failed requests with 401 status
- Call the token refresh endpoint using the refresh token
- Update stored tokens after successful refresh
- Retry the original failed request transparently
- If token refresh fails, log the user out and redirect to the login page
Constraints:
- Do not require manual user action for token refresh
- Avoid multiple simultaneous refresh calls (handle concurrent requests properly)
- Keep existing API contracts unchanged
Implement this in a clean, production-ready way.

Create an apropriate favicon for the app

Refactor the existing Home page of the FoodAdviser frontend application to look like a production SaaS landing page.
Goals:
- Make the Home page visually appealing, modern, and welcoming
- Clearly communicate the purpose of the app (food inventory + recipe assistant)
- Guide the user to the main actions of the application
UX requirements:
- Add a clear hero section with a title, short description, and primary call-to-action
- Highlight key features (e.g. manage products, scan receipts, generate recipes)
- Use clear visual hierarchy and spacing
- Keep the page clean, minimal, and easy to scan
- Ensure responsive behavior for mobile and desktop
Visual style:
- Production SaaS look and feel
- Calm, food-related color palette aligned with FoodAdviser theme
- Rounded cards, subtle shadows, and consistent typography
- Avoid clutter and unnecessary animations
Images:
- Add illustrative images or visuals to improve the page
- If real images are not available, use:
  - Placeholder images
  - Stock-style images
  - Or programmatically generated images (e.g. via image URLs)
- Images should be relevant to food, cooking, or productivity
- Ensure images are optional and do not block page functionality
Constraints:
- Do not change existing routing or business logic
- Keep navigation intact
- Focus only on improving the Home page UI and UX
 Refactor the Home page accordingly.
 If image generation is not supported directly, prepare the UI in a way that allows easy replacement of placeholder images with AI-generated or stock images later.

 Create a dedicated file for the home page and put it with ather pages

 Do refactoring of the Home page.
- Move Icons to dedicated components folder
- Separate into logical, reusable components
- Use map() for cleaner code for features, metrics, and steps

Rename the folder dtos to models. Rename interfaces in that folder, use a suffix Model instead of Dto.


Refactor authentication error handling of the FoodAdviser frontend.
Problem:
API errors are currently displayed as raw JSON, which is not user-friendly.
Goals:
- Properly parse API error responses
- Display clean, human-readable error messages
- Hide technical details such as status codes and JSON structure
Requirements:
- Extract the error message from the API response (e.g. detail or message field)
- Display a friendly message like:  "Invalid email or password."
- Do not display raw JSON or technical fields (title, status, traceId, etc.)
UI behavior:
- Show the error in a styled alert or inline form message
- Keep user-entered email intact
- Highlight the form as invalid if authentication fails
- Allow the user to retry immediately
Technical details:
- Update the API error parsing logic in the login request
- Normalize error responses if needed
- Ensure try/catch handles fetch error structures correctly
Do not modify backend responses — adapt the frontend only.

Added a delete confirmation modal and wired delete buttons to open it instead of using confirm()

#file:frontend
Create a production-ready Dockerfile at frontend/Dockerfile for a React + TypeScript application built with Vite. Use pnpm for dependencies.

Create a GitHub Actions workflow file at `.github/workflows/frontend-ci.yml` for a frontend project using **React + Vite + TypeScript + pnpm**.
The workflow must follow CI best practices, include dependent jobs, caching, artifact publishing, and Docker image build & push.
Do NOT include any test jobs.

---
## Triggers
Run workflow on:
* Push to:
  * `main`
  * `develop`
  * `feature/**`
* Pull Requests targeting `main`
Use path filters so workflow runs only when:
* `frontend/**` changes, OR
* Workflow file changes
---
## General Requirements
* Use concurrency control to cancel outdated runs
* Follow GitHub Actions + pnpm + Docker best practices
---
## Jobs Overview

1. install
2. lint-and-format (non-blocking)
3. build
4. publish-artifact
5. docker-build-and-push
Use `needs` dependencies.
---
## 1️⃣ install
## 2️⃣ lint-and-format
**Depends on:** `install`
Must NOT block pipeline.
Report errors but do not fail workflow.
---
## 3️⃣ build
**Depends on:** `install`
Steps:
* Checkout
* Setup Node
* Setup pnpm
* Restore deps
### Mandatory clean step
## 4️⃣ publish-artifact
**Depends on:** `build`
Upload Vite build output.
Artifact must be deployable static build.
---
## 5️⃣ docker-build-and-push
**Depends on:** `build`
Purpose: Build and upload Docker image.
---
### Registry
Use GitHub Container Registry (GHCR):
Image must contain production build only.
## Caching & Optimization
* pnpm dependency cache
* Docker layer cache (GHA)
* Reuse install outputs
* Avoid reinstall/build duplication
---
## Best Practices
* Use `needs` dependencies
* Fail-fast default
* Non-blocking lint job
* Clean before build
* Production-only artifacts
* Official actions only:
  * `actions/checkout`
  * `actions/setup-node`
  * `pnpm/action-setup`
  * `actions/cache`
  * `actions/upload-artifact`
