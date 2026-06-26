namespace SimpleCheckout.Domain;

public interface ITaxCalculator
{
    CalculationType Type { get; }
    bool Applies(PricingRule rule, IReadOnlyList<OrderItem> items, decimal subtotal);
    decimal Apply(PricingRule rule, IReadOnlyList<OrderItem> items, decimal subtotal);
}
