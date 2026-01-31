
Create a page for uploading receipt images and displaying extracted products returned by the API. Do not remove receipt image after analyzing. Show a loading spinner (circle) while the receipt is being analyzed/uploaded.

Create an Inventory page that displays the list of products available in the user inventory.
The page should:
-Load inventory data from the FoodManager API on page load
-Display product name and quantity in a table or list
-Handle loading and empty states (no products available)
-Use TypeScript types based on existing API contracts
-Use the centralized API client from the api folder
The page represents the user’s current food inventory stored in the database.

Create for DTOS a dedicated folder and put each dto to a separete file

Generate TypeScript interfaces based on the backend DTOs used by the FoodAdviser API.

Create a file with insructions for GitHub Copilot fronted part of  FoodAdviser application. FoodAdviser API has been written on .Net 10 and stored in the backend folder
 Use this as a base information: 
React application (Vite + TypeScript) for FoodAdviser frontend.
This app consumes an existing ASP.NET Core Web API defined in docs/openapi.json. 
instruction file : /fronend/.github/copilot-instructions.md

Create a React application (Vite + TypeScript) for FoodManager frontend.
This app consumes an existing ASP.NET Core Web API defined in docs/openapi.json.