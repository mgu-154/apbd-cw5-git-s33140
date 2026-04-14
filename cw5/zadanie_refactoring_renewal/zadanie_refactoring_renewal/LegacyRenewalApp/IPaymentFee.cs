namespace LegacyRenewalApp;

public interface IPaymentFee
{
    (decimal Fee, string Note) Calculate(string paymentMethod, decimal amount);
}