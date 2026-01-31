import { ApiClient } from './http';
import type { CreateFoodItemDto, FoodItemDto } from './dtos';

export type { CreateFoodItemDto, FoodItemDto };

export class InventoryApi {
  private readonly client = new ApiClient();

  list(params?: { page?: number; pageSize?: number; signal?: AbortSignal }): Promise<FoodItemDto[]> {
    const page = params?.page ?? 1;
    const pageSize = params?.pageSize ?? 20;
    const query = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
    return this.client.get<FoodItemDto[]>(`/api/Inventory?${query.toString()}`, params?.signal);
  }

  create(dto: CreateFoodItemDto, signal?: AbortSignal): Promise<FoodItemDto> {
    return this.client.postJson<FoodItemDto>('/api/Inventory', dto, signal);
  }

  update(id: string, dto: FoodItemDto, signal?: AbortSignal): Promise<void> {
    return this.client.putJson<void>(`/api/Inventory/${encodeURIComponent(id)}`, dto, signal);
  }

  remove(id: string, signal?: AbortSignal): Promise<void> {
    return this.client.delete<void>(`/api/Inventory/${encodeURIComponent(id)}`, signal);
  }
}
