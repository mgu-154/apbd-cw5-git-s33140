namespace LegacyRenewalApp;

public class LoyaltyPointsDiscount : IDiscountLevel
{
    public (decimal Amount, string Note) Apply(Customer customer, SubscriptionPlan subscriptionPlan, decimal amount,
        int seatCount,
        bool useLoyaltyPoints)
    {
        if (useLoyaltyPoints && customer.LoyaltyPoints > 0)
        {
            int pointsToUse = customer.LoyaltyPoints > 200 ? 200 : customer.LoyaltyPoints;
            return (pointsToUse, $"loyalty points used: {pointsToUse}; ");
        }
        return (0m, string.Empty);
    }
}