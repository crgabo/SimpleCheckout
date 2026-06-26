namespace SimpleCheckout.Infrastructure;

using Microsoft.EntityFrameworkCore;
using SimpleCheckout.Domain;

public class EfCheckoutRepository(SimpleCheckoutDbContext context) : ICheckoutRepository
{
    public async Task<CheckoutOrder> CreateAsync(CheckoutOrder order)
    {
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        return order;
    }

    public async Task<CheckoutOrder?> GetByIdAsync(Guid id) =>
        await context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
}
