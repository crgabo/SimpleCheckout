namespace SimpleCheckout.Application;

using SimpleCheckout.Domain;

public class CheckoutService(
    ICheckoutRepository repository,
    IPricingRuleRepository pricingRules,
    IPricingCalculator calculator,
    IEnumerable<IValidator<CheckoutRequest>> validators)
{
    public async Task<CheckoutResponse> CreateAsync(CheckoutRequest request)
    {
        var errors = validators.SelectMany(v => v.Validate(request)).ToList();
        if (errors.Count > 0) throw new ValidationException(errors);

        var items = request.Items
            .Select(i => new OrderItem { Name = i.Name, Quantity = i.Quantity, Price = i.UnitPrice })
            .ToList();

        var subtotal = items.Sum(i => i.Price * i.Quantity);
        var rules = await pricingRules.GetAllAsync();

        var (discount, taxes) = calculator.Calculate(rules, items, subtotal);

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

    public async Task<List<CheckoutListResponse>> GetLatestAsync(int count = 20)
    {
        var orders = await repository.GetLatestAsync(count);
        return orders.Select(ToListResponse).ToList();
    }

    private static CheckoutResponse ToResponse(CheckoutOrder order) =>
        new(order.Id, order.Subtotal, order.Discount, order.Taxes, order.Total);

    private static CheckoutListResponse ToListResponse(CheckoutOrder order) =>
        new(
            order.Id,
            order.Subtotal,
            order.Discount,
            order.Taxes,
            order.Total,
            order.CreatedAt,
            order.Items.Select(i => new OrderItemDto(i.Name, i.Price, i.Quantity)).ToList()
        );
}
