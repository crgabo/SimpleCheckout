namespace SimpleCheckout.Domain;

public interface ICheckoutRepository
{
    Task<CheckoutOrder> CreateAsync(CheckoutOrder order);
    Task<CheckoutOrder?> GetByIdAsync(Guid id);
}
