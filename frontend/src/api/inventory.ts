import { ApiClient } from './http';
import type { CreateFoodItemModel, FoodItemModel } from './models';

export type { CreateFoodItemModel, FoodItemModel };

export class InventoryApi {
  private readonly client: ApiClient;

  constructor(client?: ApiClient) {
    this.client = client ?? new ApiClient();
  }

  list(params?: { page?: number; pageSize?: number; signal?: AbortSignal }): Promise<FoodItemModel[]> {
    const page = params?.page ?? 1;
    const pageSize = params?.pageSize ?? 20;
    const query = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
    return this.client.get<FoodItemModel[]>(`/api/Inventory?${query.toString()}`, params?.signal);
  }

  create(dto: CreateFoodItemModel, signal?: AbortSignal): Promise<FoodItemModel> {
    return this.client.postJson<FoodItemModel>('/api/Inventory', dto, signal);
  }

  update(id: string, dto: FoodItemModel, signal?: AbortSignal): Promise<void> {
    return this.client.putJson<void>(`/api/Inventory/${encodeURIComponent(id)}`, dto, signal);
  }

  remove(id: string, signal?: AbortSignal): Promise<void> {
    return this.client.delete<void>(`/api/Inventory/${encodeURIComponent(id)}`, signal);
  }
}
