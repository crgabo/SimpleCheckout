namespace SimpleCheckout.Application.Validators;

using SimpleCheckout.Domain;

public class CheckoutRequestValidator : IValidator<CheckoutRequest>
{
    public IReadOnlyList<string> Validate(CheckoutRequest request)
    {
        var errors = new List<string>();

        if (request.Items is null || request.Items.Count == 0)
        {
            errors.Add("Items list cannot be empty.");
            return errors;
        }

        foreach (var item in request.Items)
        {
            if (string.IsNullOrWhiteSpace(item.Name))
                errors.Add("Item name is required.");
            if (item.UnitPrice <= 0)
                errors.Add($"'{item.Name}': unit price must be greater than 0.");
            if (item.Quantity <= 0)
                errors.Add($"'{item.Name}': quantity must be greater than 0.");
        }

        return errors;
    }
}
