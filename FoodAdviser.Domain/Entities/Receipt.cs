namespace FoodAdviser.Domain.Entities;

/// <summary>
/// Represents a shopping receipt with line items.
/// </summary>
public class Receipt
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string Description { get; set; } = string.Empty;
    public List<ReceiptLineItem> Items { get; set; } = new();
}

/// <summary>
/// Line item in a receipt.
/// </summary>
public class ReceiptLineItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
