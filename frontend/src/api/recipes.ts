import { ApiClient } from './http';
import type {
  ConfirmRecipesRequestModel,
  ConfirmRecipesResponseModel,
  GenerateRecipesRequestModel,
  RecipeModel,
} from './models';
import { DishType } from './models';

export { DishType };
export type {
  ConfirmRecipesRequestModel,
  ConfirmRecipesResponseModel,
  GenerateRecipesRequestModel,
  RecipeModel,
};

export class RecipesApi {
  private readonly client: ApiClient;

  constructor(client?: ApiClient) {
    this.client = client ?? new ApiClient();
  }

  generate(dto: GenerateRecipesRequestModel, signal?: AbortSignal): Promise<RecipeModel[]> {
    return this.client.postJson<RecipeModel[]>('/api/Recipes/generate', dto, signal);
  }

  confirm(dto: ConfirmRecipesRequestModel, signal?: AbortSignal): Promise<ConfirmRecipesResponseModel> {
    return this.client.postJson<ConfirmRecipesResponseModel>('/api/Recipes/confirm', dto, signal);
  }

  // OpenAPI marks this endpoint as a stub with unspecified response.
  suggestions(params?: { max?: number; signal?: AbortSignal }): Promise<unknown> {
    const query = new URLSearchParams({ max: String(params?.max ?? 10) });
    return this.client.get<unknown>(`/api/Recipes/suggestions?${query.toString()}`, params?.signal);
  }
}
