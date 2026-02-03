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