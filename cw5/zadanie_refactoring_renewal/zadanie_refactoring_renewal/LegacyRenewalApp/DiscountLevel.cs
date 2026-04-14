namespace LegacyRenewalApp;

public class DiscountLevel : IDiscountLevel
{
    public (decimal Amount, string Note) Apply(Customer customer, SubscriptionPlan subscriptionPlan, decimal amount,
        int seatCount,
        bool useLoyaltyPoints)
    {
        if (customer.Segment == "Silver") return (amount * 0.05m, "silver discount; ");
        if (customer.Segment == "Gold") return (amount * 0.10m, "gold discount; ");
        if (customer.Segment == "Platinum") return (amount * 0.15m, "platinum discount; ");
        if (customer.Segment == "Education" && subscriptionPlan.IsEducationEligible) return (amount * 0.20m, "education discount; ");
        return (0m, string.Empty);
    }
}