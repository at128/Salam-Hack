namespace SalamHack.Domain.Common.Constants;

public static class ApplicationConstants
{
    public const string Timezone = "UTC";

    public static class Roles
    {
        public const string Admin = "Admin";
        public const string User = "User";
    }

    public static class FieldLengths
    {
        public const int EmailMaxLength = 256;
        public const int PasswordMinLength = 8;
        public const int FirstNameMaxLength = 100;
        public const int LastNameMaxLength = 100;
        public const int PhoneNumberMaxLength = 32;
    }

    public static class BusinessRules
    {
        public const decimal TaxRate = 0.15m;
        public const decimal TargetProfitMarginRate = 0.45m;
        public const decimal HealthyMarginThreshold = 30m;
        public const decimal AtRiskMarginThreshold = 15m;
        public const decimal AdvancePaymentRate = 0.30m;
        public const decimal MinimumPriceMultiplier = 1.10m;
        public const decimal EconomyPriceMultiplier = 0.78m;
        public const decimal PremiumPriceMultiplier = 1.30m;
        public const decimal CostRatePerHour = 55m;
        public const decimal DefaultRevenueRatePerHour = 180m;
        public const decimal CostToRevenueRatio = 0.35m;
        public const decimal SimpleComplexityMultiplier = 0.80m;
        public const decimal MediumComplexityMultiplier = 1.00m;
        public const decimal ComplexComplexityMultiplier = 1.40m;
        public const decimal UrgentProjectPriceMultiplier = 1.15m;
        public const decimal ExtraRevisionPriceRate = 0.05m;
        public const decimal NewServiceConfidenceBufferRate = 0.05m;
        public const decimal MinimumHistoricalHoursFactor = 0.90m;
        public const decimal MaximumHistoricalHoursFactor = 2.50m;
        public const decimal MinimumResidualCostFactor = 0.95m;
        public const decimal MaximumResidualCostFactor = 1.50m;
    }
}
