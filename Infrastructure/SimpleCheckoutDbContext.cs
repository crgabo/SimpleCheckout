namespace SimpleCheckout.Infrastructure;

using Microsoft.EntityFrameworkCore;
using SimpleCheckout.Domain;

public class SimpleCheckoutDbContext(DbContextOptions<SimpleCheckoutDbContext> options) : DbContext(options)
{
    public DbSet<CheckoutOrder> Orders       => Set<CheckoutOrder>();
    public DbSet<PricingRule>   PricingRules => Set<PricingRule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CheckoutOrder>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Ignore(o => o.Subtotal);
            entity.Property(o => o.Taxes).HasColumnType("decimal(18,2)");
            entity.Property(o => o.Discount).HasColumnType("decimal(18,2)");
            entity.Property(o => o.Total).HasColumnType("decimal(18,2)");

            entity.OwnsMany(o => o.Items, item =>
            {
                item.Property<int>("Id").ValueGeneratedOnAdd();
                item.HasKey("Id");
                item.Property(i => i.Name);
                item.Property(i => i.Price).HasColumnType("decimal(18,2)");
                item.Property(i => i.Quantity).HasColumnType("decimal(18,4)");
            });
        });

        modelBuilder.Entity<PricingRule>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Rate).HasColumnType("decimal(18,4)");
            entity.Property(r => r.MinSubtotal).HasColumnType("decimal(18,2)");

            entity.HasData(
                new PricingRule
                {
                    Id          = Guid.Parse("a1b2c3d4-0001-0001-0001-000000000001"),
                    Name        = "VAT",
                    Type        = PricingRuleType.Tax,
                    Calculation = CalculationType.Percentage,
                    Rate        = 0.13m
                },
                new PricingRule
                {
                    Id          = Guid.Parse("a1b2c3d4-0002-0002-0002-000000000002"),
                    Name        = "Discount over 100",
                    Type        = PricingRuleType.Discount,
                    Calculation = CalculationType.Percentage,
                    Rate        = 0.10m,
                    MinSubtotal = 100m
                }
            );
        });
    }
}
