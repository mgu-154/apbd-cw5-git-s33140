namespace LegacyRenewalApp;

public interface ITaxRate
{
    decimal GetTaxRate(string country);
}