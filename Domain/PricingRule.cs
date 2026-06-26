namespace SimpleCheckout.Domain;

public enum PricingRuleType { Tax, Discount }

public enum CalculationType { Percentage }

public class PricingRule
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public PricingRuleType Type { get; init; }
    public CalculationType Calculation { get; init; }
    public decimal Rate { get; init; }
    public decimal? MinSubtotal { get; init; }
}

public interface IPricingRuleRepository
{
    Task<IReadOnlyList<PricingRule>> GetAllAsync();
}
