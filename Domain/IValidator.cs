namespace SimpleCheckout.Domain;

public interface IValidator<T>
{
    IReadOnlyList<string> Validate(T request);
}
