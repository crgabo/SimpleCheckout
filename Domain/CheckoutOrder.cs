namespace SimpleCheckout.Domain;

public class CheckoutOrder
{
    public Guid Id { get; init; }
    public List<OrderItem> Items { get; init; } = [];
    public decimal Subtotal => Items.Sum(i => i.Price * i.Quantity);
    public decimal Taxes    { get; init; }
    public decimal Discount { get; init; }
    public decimal Total    { get; init; }
    public DateTime CreatedAt { get; init; }
}

public class OrderItem
{
    public string Name { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal Price { get; init; }
}
