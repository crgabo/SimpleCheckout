namespace SimpleCheckout.Application;

using SimpleCheckout.Domain;

public class CheckoutService(
    ICheckoutRepository repository,
    IPricingRuleRepository pricingRules,
    IEnumerable<IDiscountCalculator> discountCalculators,
    IEnumerable<ITaxCalculator> taxCalculators,
    IEnumerable<IValidator<CheckoutRequest>> validators)
{
    public async Task<CheckoutResponse> CreateAsync(CheckoutRequest request)
    {
        var errors = validators.SelectMany(v => v.Validate(request)).ToList();
        if (errors.Count > 0) throw new ValidationException(errors);

        var items    = request.Items.Select(i => new OrderItem { Name = i.Name, Quantity = i.Quantity, Price = i.UnitPrice }).ToList();
        
        var subtotal = items.Sum(i => i.Price * i.Quantity);

        var rules    = await pricingRules.GetAllAsync();
        
        var discount = rules.Where(r => r.Type == PricingRuleType.Discount)
                            .Sum(r => discountCalculators
                                .Where(c => c.Type == r.Calculation && c.Applies(r, items, subtotal))
                                .Sum(c => c.Apply(r, items, subtotal)));

        var taxes    = rules.Where(r => r.Type == PricingRuleType.Tax)
                            .Sum(r => taxCalculators
                                .Where(c => c.Type == r.Calculation && c.Applies(r, items, subtotal))
                                .Sum(c => c.Apply(r, items, subtotal - discount)));

        var order = new CheckoutOrder
        {
            Id        = Guid.NewGuid(),
            Items     = items,
            Taxes     = taxes,
            Discount  = discount,
            Total     = subtotal + taxes - discount,
            CreatedAt = DateTime.UtcNow
        };

        var created = await repository.CreateAsync(order);
        return ToResponse(created);
    }

    public async Task<CheckoutResponse?> GetByIdAsync(Guid id)
    {
        var order = await repository.GetByIdAsync(id);
        return order is null ? null : ToResponse(order);
    }

    private static CheckoutResponse ToResponse(CheckoutOrder order) =>
        new(order.Id, order.Subtotal, order.Discount, order.Taxes, order.Total);
}
