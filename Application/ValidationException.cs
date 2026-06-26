namespace SimpleCheckout.Application;

public class ValidationException(IReadOnlyList<string> errors) : Exception
{
    public IReadOnlyList<string> Errors { get; } = errors;
}
