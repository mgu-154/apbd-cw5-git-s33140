namespace LegacyRenewalApp;

public interface IDiscountLevel
{
    (decimal Amount, string Note) Apply(Customer customer, SubscriptionPlan subscriptionPlan, decimal amount, int seatCount, bool useLoyaltyPoints);
}