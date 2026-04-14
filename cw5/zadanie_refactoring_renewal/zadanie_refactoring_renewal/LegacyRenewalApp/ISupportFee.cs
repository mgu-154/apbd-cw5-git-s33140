namespace LegacyRenewalApp;

public interface ISupportFee
{
    (decimal Fee, string Note) Calculate(string planCode, bool includePremiumSupport);
}