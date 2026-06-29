namespace SimpleCheckout.Application.Rules;

using SimpleCheckout.Domain;

public class PricingCalculator : IPricingCalculator
{
    private readonly IEnumerable<IDiscountCalculator> _discountCalculators;
    private readonly IEnumerable<ITaxCalculator> _taxCalculators;

    public PricingCalculator(
        IEnumerable<IDiscountCalculator> discountCalculators,
        IEnumerable<ITaxCalculator> taxCalculators)
    {
        _discountCalculators = discountCalculators;
        _taxCalculators = taxCalculators;
    }

    public (decimal Discount, decimal Taxes) Calculate(
        IReadOnlyList<PricingRule> rules,
        IReadOnlyList<OrderItem> items,
        decimal subtotal)
    {
        var discount = rules.Where(r => r.Type == PricingRuleType.Discount)
                            .Sum(r => _discountCalculators
                                .Where(c => c.Type == r.Calculation && c.Applies(r, items, subtotal))
                                .Sum(c => c.Apply(r, items, subtotal)));

        var taxes = rules.Where(r => r.Type == PricingRuleType.Tax)
                         .Sum(r => _taxCalculators
                            .Where(c => c.Type == r.Calculation && c.Applies(r, items, subtotal - discount))
                            .Sum(c => c.Apply(r, items, subtotal - discount)));

        return (discount, taxes);
    }
}
