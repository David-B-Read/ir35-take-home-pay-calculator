namespace ContractorTakeHomePayCalculator.Api.Models
{
    public class TakeHomePayCalculationBreakdown
    {
        public decimal AssignmentRate { get; set; }
        public decimal MonthlyFee { get; set; }
        public decimal AfterMonthlyFee { get; set; }

        public decimal EmployerNI { get; set; }
        public decimal ApprenticeshipLevy { get; set; }
        public decimal TotalEmployerCosts { get; set; }

        public decimal EmploymentCostBase { get; set; }

        public decimal SalarySacrificePension { get; set; }
        public decimal TaxablePay { get; set; }

        public decimal IncomeTax { get; set; }
        public decimal EmployeeNI { get; set; }

        public decimal NetTakeHomePay { get; set; }
    }

}
