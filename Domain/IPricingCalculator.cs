namespace SimpleCheckout.Domain;

public interface IPricingCalculator
{
    (decimal Discount, decimal Taxes) Calculate(
        IReadOnlyList<PricingRule> rules,
        IReadOnlyList<OrderItem> items,
        decimal subtotal);
}
