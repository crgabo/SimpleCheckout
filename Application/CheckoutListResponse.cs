namespace SimpleCheckout.Application;

public record OrderItemDto(
    string Name,
    decimal Price,
    decimal Quantity
);

public record CheckoutListResponse(
    Guid Id,
    decimal Subtotal,
    decimal Discount,
    decimal Taxes,
    decimal Total,
    DateTime CreatedAt,
    List<OrderItemDto> Items
);
