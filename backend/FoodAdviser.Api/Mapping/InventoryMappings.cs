using FoodAdviser.Application.DTOs.Inventory;
using FoodAdviser.Domain.Entities;

namespace FoodAdviser.Api.Mapping;

public static class InventoryMappings
{
    public static FoodItemDto ToDto(this FoodItem item) => new()
    {
        Id = item.Id,
        Name = item.Name,
        Quantity = item.Quantity,
        Unit = item.Unit,
        ExpiresAt = item.ExpiresAt
    };

    public static FoodItem ToEntity(this FoodItemDto dto, Guid userId) => new()
    {
        Id = dto.Id,
        UserId = userId,
        Name = dto.Name,
        Quantity = dto.Quantity,
        Unit = dto.Unit,
        ExpiresAt = dto.ExpiresAt
    };

    public static FoodItem ToEntity(this CreateFoodItemDto dto, Guid userId) => new()
    {
        UserId = userId,
        Name = dto.Name,
        Quantity = dto.Quantity,
        Unit = dto.Unit,
        ExpiresAt = dto.ExpiresAt
    };
}
