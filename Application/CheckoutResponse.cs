namespace SimpleCheckout.Application;

public record CheckoutResponse(
    Guid Id,
    decimal Subtotal,
    decimal Discount,
    decimal Taxes,
    decimal Total
);