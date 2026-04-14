namespace LegacyRenewalApp;

public class DiscountService : IDiscountService
{
    private readonly IEnumerable<IDiscountLevel> _discountLevels;

    public DiscountService(IEnumerable<IDiscountLevel> discountLevels)
    {
        _discountLevels = discountLevels;
    }

    public (decimal TotalDiscount, string Note) CalculateDiscount(Customer customer, SubscriptionPlan subscriptionPlan,
        decimal amount, int seatCount, bool useLoyaltyPoints)
    {
        decimal totalDiscount = 0m;
        string totalNotes = string.Empty;

        foreach (var discountLevel in _discountLevels)
        {
            var result = discountLevel.Apply(customer, subscriptionPlan, amount, seatCount, useLoyaltyPoints);
            totalDiscount += result.Amount;
            totalNotes += result.Note;
        }
        return (totalDiscount, totalNotes);
    }
}