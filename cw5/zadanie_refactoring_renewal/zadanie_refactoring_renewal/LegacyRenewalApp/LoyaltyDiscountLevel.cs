namespace LegacyRenewalApp;

public class LoyaltyDiscountLevel : IDiscountLevel
{
    public (decimal Amount, string Note) Apply(Customer customer, SubscriptionPlan subscriptionPlan, decimal amount, int seatCount,
        bool useLoyaltyPoints)
    {
        if (customer.YearsWithCompany >= 5) return (amount * 0.07m, "long-term loyalty discount; ");
        if (customer.YearsWithCompany >= 2) return (amount * 0.03m, "basic loyalty discount; ");
        return (0m, string.Empty);
    }
}