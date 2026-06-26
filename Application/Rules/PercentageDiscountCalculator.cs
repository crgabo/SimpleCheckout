namespace SimpleCheckout.Application.Rules;

using SimpleCheckout.Domain;

public class PercentageDiscountCalculator : IDiscountCalculator
{
    public CalculationType Type => CalculationType.Percentage;

    public bool Applies(PricingRule rule, IReadOnlyList<OrderItem> items, decimal subtotal)
        => rule.MinSubtotal is not null && subtotal > rule.MinSubtotal.Value;

    public decimal Apply(PricingRule rule, IReadOnlyList<OrderItem> items, decimal subtotal)
        => Math.Round(subtotal * rule.Rate, 2);
}
