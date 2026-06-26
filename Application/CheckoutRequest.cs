namespace SimpleCheckout.Application;

public record CheckoutRequest(
    List<OrderItemRequest> Items
);

public record OrderItemRequest(
    string Name,
    decimal UnitPrice,
    decimal Quantity
);
