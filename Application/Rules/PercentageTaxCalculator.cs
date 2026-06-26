namespace SimpleCheckout.Application.Rules;

using SimpleCheckout.Domain;

public class PercentageTaxCalculator : ITaxCalculator
{
    public CalculationType Type => CalculationType.Percentage;

    public bool Applies(PricingRule rule, IReadOnlyList<OrderItem> items, decimal subtotal)
        => true;

    public decimal Apply(PricingRule rule, IReadOnlyList<OrderItem> items, decimal subtotal)
        => Math.Round(subtotal * rule.Rate, 2);
}
