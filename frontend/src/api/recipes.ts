import { ApiClient } from './http';
import type {
  ConfirmRecipesRequestDto,
  ConfirmRecipesResponseDto,
  GenerateRecipesRequestDto,
  RecipeDto,
} from './dtos';
import { DishType } from './dtos';

export { DishType };
export type {
  ConfirmRecipesRequestDto,
  ConfirmRecipesResponseDto,
  GenerateRecipesRequestDto,
  RecipeDto,
};

export class RecipesApi {
  private readonly client = new ApiClient();

  generate(dto: GenerateRecipesRequestDto, signal?: AbortSignal): Promise<RecipeDto[]> {
    return this.client.postJson<RecipeDto[]>('/api/Recipes/generate', dto, signal);
  }

  confirm(dto: ConfirmRecipesRequestDto, signal?: AbortSignal): Promise<ConfirmRecipesResponseDto> {
    return this.client.postJson<ConfirmRecipesResponseDto>('/api/Recipes/confirm', dto, signal);
  }

  // OpenAPI marks this endpoint as a stub with unspecified response.
  suggestions(params?: { max?: number; signal?: AbortSignal }): Promise<unknown> {
    const query = new URLSearchParams({ max: String(params?.max ?? 10) });
    return this.client.get<unknown>(`/api/Recipes/suggestions?${query.toString()}`, params?.signal);
  }
}
