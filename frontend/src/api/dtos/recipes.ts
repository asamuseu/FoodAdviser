import type { Guid } from './common';
import type { DishType } from './enums';

export interface IngredientDto {
  name: string;
  quantity: number;
  unit: string;
}

export interface RecipeDto {
  id: Guid;
  title: string;
  description: string;
  dishType: DishType;
  ingredients: IngredientDto[];
}

export interface GenerateRecipesRequestDto {
  dishType: DishType;
  numberOfPersons: number;
}

export interface ConfirmRecipesRequestDto {
  recipeIds: Guid[];
}

export interface InventoryUpdateDto {
  productName: string;
  previousQuantity: number;
  usedQuantity: number;
  newQuantity: number;
  unit: string;
}

export interface ConfirmRecipesResponseDto {
  success: boolean;
  message: string;
  confirmedRecipesCount: number;
  inventoryUpdates: InventoryUpdateDto[];
}
