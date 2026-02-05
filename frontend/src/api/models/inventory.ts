import type { Guid, IsoDateTime } from './common';

export interface FoodItemModel {
  id: Guid;
  name: string;
  quantity: number;
  unit: string;
  expiresAt?: IsoDateTime | null;
}

export interface CreateFoodItemModel {
  name: string;
  quantity: number;
  unit: string;
  expiresAt?: IsoDateTime | null;
}
