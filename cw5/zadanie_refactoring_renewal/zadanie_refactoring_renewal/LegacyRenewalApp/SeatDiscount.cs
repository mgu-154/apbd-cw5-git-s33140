namespace LegacyRenewalApp;

public class SeatDiscount : IDiscountLevel
{
    public (decimal Amount, string Note) Apply(Customer customer, SubscriptionPlan subscriptionPlan, decimal amount,
        int seatCount,
        bool useLoyaltyPoints)
    {
        if (seatCount >= 50) return (0.12m, "large team discount; ");
        if (seatCount >= 20) return (0.08m, "medium team discount; ");
        if (seatCount >= 10) return (0.04m, "small team discount; ");
        return (0m, string.Empty);
    }
}