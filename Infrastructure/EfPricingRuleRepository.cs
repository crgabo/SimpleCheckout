namespace SimpleCheckout.Infrastructure;

using Microsoft.EntityFrameworkCore;
using SimpleCheckout.Domain;

public class EfPricingRuleRepository(SimpleCheckoutDbContext context) : IPricingRuleRepository
{
    public async Task<IReadOnlyList<PricingRule>> GetAllAsync() =>
        await context.PricingRules.ToListAsync();
}
