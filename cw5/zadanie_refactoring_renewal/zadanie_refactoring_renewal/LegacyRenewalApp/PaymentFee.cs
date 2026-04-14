namespace LegacyRenewalApp;

public class PaymentFee
{
    public (decimal Fee, string Note) Calculate(string paymentMethod, decimal amount)
    {
        return paymentMethod switch
        {
            "CARD" => (amount * 0.02m, "card payment fee; "),
            "BANK_TRANSFER" => (amount * 0.01m, "bank transfer fee; "),
            "PAYPAL" => (amount * 0.035m, "paypal fee; "),
            "INVOICE" => (0m, "invoice payment; "),
            _ => throw new NotImplementedException()
        };
    }
}