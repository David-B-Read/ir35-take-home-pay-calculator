namespace ContractorTakeHomePayCalculator.Api.Configuration
{
    public class TakeHomePayCalculatorConfiguration
    {
        public decimal PersonalAllowance { get; set; }
        public decimal BasicRateLimit { get; set; }
        public decimal HigherRateLimit { get; set; }
        public decimal BasicRateTax { get; set; }
        public decimal HigherRateTax { get; set; }
        public decimal AdditionalRateTax { get; set; }
        public decimal NIPrimaryThreshold { get; set; }
        public decimal NIUpperEarningsLimit { get; set; }
        public decimal NIStandardRate { get; set; }
        public decimal NIUpperRate { get; set; }
        public decimal EmployerNIThreshold { get; set; }
        public decimal EmployerNIRate { get; set; }
        public decimal ApprenticeshipLevyRate { get; set; }
    }
}
