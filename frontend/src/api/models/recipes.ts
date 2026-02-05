import type { Guid } from './common';
import type { DishType } from './enums';

export interface IngredientModel {
  name: string;
  quantity: number;
  unit: string;
}

export interface RecipeModel {
  id: Guid;
  title: string;
  description: string;
  dishType: DishType;
  ingredients: IngredientModel[];
}

export interface GenerateRecipesRequestModel {
  dishType: DishType;
  numberOfPersons: number;
}

export interface ConfirmRecipesRequestModel {
  recipeIds: Guid[];
}

export interface InventoryUpdateModel {
  productName: string;
  previousQuantity: number;
  usedQuantity: number;
  newQuantity: number;
  unit: string;
}

export interface ConfirmRecipesResponseModel {
  success: boolean;
  message: string;
  confirmedRecipesCount: number;
  inventoryUpdates: InventoryUpdateModel[];
}
