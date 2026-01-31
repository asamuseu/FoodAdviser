import type { Guid, IsoDateTime } from './common';

export interface FoodItemDto {
  id: Guid;
  name: string;
  quantity: number;
  unit: string;
  expiresAt?: IsoDateTime | null;
}

export interface CreateFoodItemDto {
  name: string;
  quantity: number;
  unit: string;
  expiresAt?: IsoDateTime | null;
}
