using System;

namespace LegacyRenewalApp
{
    public class SubscriptionRenewalService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
        private readonly IDiscountService _discountService;
        private readonly ISupportFee _supportFee;
        private readonly IPaymentFee _paymentFee;
        private readonly ITaxRate _taxRate;
        private readonly  IBillingGateway _billingGateway;
        
        public SubscriptionRenewalService() : this(
            new CustomerRepository(),
            new SubscriptionPlanRepository(),
            new DiscountService(new List<IDiscountLevel>
                {
                    new DiscountLevel(),
                    new LoyaltyDiscountLevel(),
                    new SeatDiscount(),
                    new LoyaltyPointsDiscount()
                }),
            new SupportFee(),
            new PaymentFee(),
            new TaxRate(),
            new BillingGateway())
            {
            }

            public SubscriptionRenewalService(
                ICustomerRepository customerRepository,
                ISubscriptionPlanRepository subscriptionPlanRepository,
                IDiscountService discountService,
                ISupportFee supportFee,
                IPaymentFee paymentFee,
                ITaxRate taxRate,
                IBillingGateway billingGateway)
            {
                _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
                _subscriptionPlanRepository = subscriptionPlanRepository ?? throw new ArgumentNullException(nameof(subscriptionPlanRepository));
                _discountService = discountService ?? throw new ArgumentNullException(nameof(discountService));
                _supportFee = supportFee ?? throw new ArgumentNullException(nameof(supportFee));
                _paymentFee = paymentFee ?? throw new ArgumentNullException(nameof(paymentFee));
                _taxRate = taxRate  ?? throw new ArgumentNullException(nameof(taxRate));
                _billingGateway = billingGateway ?? throw new ArgumentNullException(nameof(billingGateway));
            }

            public RenewalInvoice CreateRenewalInvoice(
                int customerId,
                string planCode,
                int seatCount,
                string paymentMethod,
                bool includePremiumSupport,
                bool useLoyaltyPoints)
            {
                ValidateInputs(customerId, planCode, seatCount, paymentMethod);
                
                string normalizedPlanCode = planCode.Trim().ToUpperInvariant();
                string normalizedPaymentMethod = paymentMethod.Trim().ToUpperInvariant();

                var customer = _customerRepository.GetById(customerId);
                var plan = _subscriptionPlanRepository.GetByCode(normalizedPlanCode);

                if (!customer.IsActive)
                {
                    throw new InvalidOperationException("Inactive customers cannot renew subscriptions");
                }

                decimal baseAmount = (plan.MonthlyPricePerSeat * seatCount * 12m) + plan.SetupFee;
                string notes = string.Empty;

                var discountResult =
                    _discountService.CalculateDiscount(customer, plan, baseAmount, seatCount, useLoyaltyPoints);
                decimal discountAmount = discountResult.TotalDiscount;
                notes += discountResult.Note;
                
                decimal valueAfterDiscount = baseAmount - discountAmount;
                if (valueAfterDiscount < 300m)
                {
                    valueAfterDiscount = 300m;
                    notes += "minimum discounted subtotal applied; ";
                }

                var supportResult = _supportFee.Calculate(normalizedPlanCode, includePremiumSupport);
                decimal supportFee = supportResult.Fee;
                notes += supportResult.Note;
                
                var paymentResult = _paymentFee.Calculate(normalizedPaymentMethod, valueAfterDiscount + supportFee);
                decimal paymentFee = paymentResult.Fee;
                notes += paymentResult.Note;
                
                decimal taxRate = _taxRate.GetTaxRate(customer.Country);
                decimal taxBase = valueAfterDiscount + supportFee + paymentFee;
                decimal taxAmount = taxBase * taxRate;
                decimal finalAmount = taxBase + taxAmount;

                if (finalAmount < 500m)
                {
                    finalAmount = 500m;
                    notes += "minimum invoice amount applied; ";
                }
                
                var invoice = BuildInvoice(customerId, customer, normalizedPlanCode, normalizedPaymentMethod, seatCount, baseAmount, discountAmount, supportFee, paymentFee, taxAmount, finalAmount, notes);
                
                _billingGateway.SaveInvoice(invoice);

                if (!string.IsNullOrWhiteSpace(customer.Email))
                {
                    string subject = "Subscription renewal invoice";
                    string body =
                        $"Hello {customer.FullName}, your renewal for plan {normalizedPlanCode} " +
                        $"has been prepared. Final amount: {invoice.FinalAmount:F2}.";

                    _billingGateway.SendEmail(customer.Email, subject, body);
                }

                return invoice;
            }

            private static void ValidateInputs(int customerId, string planCode, int seatCount, string paymentMethod)
            {
                if (customerId <= 0) throw new ArgumentException("Customer id must be positive");
                if (string.IsNullOrWhiteSpace(planCode)) throw new ArgumentException("Plan code is required");
                if (seatCount <= 0) throw new ArgumentException("Seat count must be positive");
                if (string.IsNullOrWhiteSpace(paymentMethod)) throw new ArgumentException("Payment method is required");
            }

            private static RenewalInvoice BuildInvoice(int customerId, Customer customer, string planCode,
                string paymentMethod,
                int seatCount, decimal baseAmount, decimal discountAmount, decimal supportFee, decimal paymentFee,
                decimal taxAmount, decimal finalAmount, string notes)
            {
                return new RenewalInvoice
                {
                    InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{customerId}-{planCode}",
                    CustomerName = customer.FullName,
                    PlanCode = planCode,
                    PaymentMethod = paymentMethod,
                    SeatCount = seatCount,
                    BaseAmount = Math.Round(baseAmount, 2, MidpointRounding.AwayFromZero),
                    DiscountAmount = Math.Round(discountAmount, 2, MidpointRounding.AwayFromZero),
                    SupportFee = Math.Round(supportFee, 2, MidpointRounding.AwayFromZero),
                    PaymentFee = Math.Round(paymentFee, 2, MidpointRounding.AwayFromZero),
                    TaxAmount = Math.Round(taxAmount, 2, MidpointRounding.AwayFromZero),
                    FinalAmount = Math.Round(finalAmount, 2, MidpointRounding.AwayFromZero),
                    Notes = notes.Trim(),
                    GeneratedAt = DateTime.UtcNow
                };
            }
    }
}
