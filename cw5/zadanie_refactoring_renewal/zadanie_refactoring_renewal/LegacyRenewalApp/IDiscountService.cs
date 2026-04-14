namespace LegacyRenewalApp;

public interface IDiscountService
{
    (decimal TotalDiscount, string Note) CalculateDiscount(Customer customer, SubscriptionPlan subscriptionPlan, decimal amount,
        int seatCount,
        bool useLoyaltyPoints);
}