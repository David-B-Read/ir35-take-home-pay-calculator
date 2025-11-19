using ContractorTakeHomePayCalculator.Api.Models;

namespace ContractorTakeHomePayCalculator.Api.Services
{
    public class TakeHomePayCalculatorService
    {
        private const decimal PersonalAllowance = 12570m;
        private const decimal BasicRateLimit = 50270m;
        private const decimal HigherRateLimit = 125140m;

        private const decimal BasicRateTax = 0.20m;
        private const decimal HigherRateTax = 0.40m;
        private const decimal AdditionalRateTax = 0.45m;

        private const decimal NIPrimaryThreshold = 12568m;
        private const decimal NIUpperEarningsLimit = 50270m;
        private const decimal NIStandardRate = 0.08m;
        private const decimal NIUpperRate = 0.02m;

        private const decimal EmployerNIThreshold = 9100m;
        private const decimal EmployerNIRate = 0.15m;

        private const decimal ApprenticeshipLevyRate = 0.005m;

        public TakeHomePayCalculationBreakdown CalculateBreakdown(
            decimal dayRate,
            int daysWorked,
            decimal monthlyFee,
            decimal salarySacrificePension)
        {
            var breakdown = new TakeHomePayCalculationBreakdown
            {
                AssignmentRate = dayRate * daysWorked,
                MonthlyFee = monthlyFee
            };
            breakdown.AfterMonthlyFee = breakdown.AssignmentRate - monthlyFee;

            breakdown.EmployerNI = CalculateEmployerNI(breakdown.AfterMonthlyFee);
            breakdown.ApprenticeshipLevy = breakdown.AfterMonthlyFee * ApprenticeshipLevyRate;
            breakdown.TotalEmployerCosts = breakdown.EmployerNI + breakdown.ApprenticeshipLevy;

            breakdown.EmploymentCostBase = breakdown.AfterMonthlyFee - breakdown.TotalEmployerCosts;
            if (breakdown.EmploymentCostBase < 0)
            {
                breakdown.EmploymentCostBase = 0;
            }
            
            breakdown.SalarySacrificePension = salarySacrificePension;
            breakdown.TaxablePay = Math.Max(0, breakdown.EmploymentCostBase - salarySacrificePension);

            breakdown.IncomeTax = CalculateIncomeTax(breakdown.TaxablePay);
            breakdown.EmployeeNI = CalculateEmployeeNI(breakdown.TaxablePay);

            breakdown.NetTakeHomePay = breakdown.TaxablePay - breakdown.IncomeTax - breakdown.EmployeeNI;

            return breakdown;
        }

        private decimal CalculateEmployerNI(decimal pay)
        {
            var monthlyNIThreshold = EmployerNIThreshold / 12;

            if (pay <= monthlyNIThreshold) return 0;
            return (pay - monthlyNIThreshold) * EmployerNIRate;
        }

        private decimal CalculateIncomeTax(decimal taxablePay)
        {
            decimal tax = 0;
            decimal remaining = taxablePay;

            decimal adjustedPA = PersonalAllowance / 12;
            if (taxablePay > (100000 / 12))
                adjustedPA = Math.Max(0, (PersonalAllowance/12) - (taxablePay - (100000 / 12)) / 2);

            remaining -= adjustedPA;
            if (remaining <= 0) return 0;

            decimal basicBand = Math.Min(remaining, (BasicRateLimit / 12) - adjustedPA);
            tax += basicBand * BasicRateTax;
            remaining -= basicBand;
            if (remaining <= 0) return tax;

            decimal higherBand = Math.Min(remaining, (HigherRateLimit - BasicRateLimit) / 12);
            tax += higherBand * HigherRateTax;
            remaining -= higherBand;
            if (remaining <= 0) return tax;

            tax += remaining * AdditionalRateTax;

            return tax;
        }

        private decimal CalculateEmployeeNI(decimal taxablePay)
        {
            var monthlyNIPrimaryThreshold = NIPrimaryThreshold / 12;
            var monthlyNIUpperEarningsLimit = NIUpperEarningsLimit / 12; 

            if (taxablePay <= monthlyNIPrimaryThreshold)
                return 0;

            decimal ni = 0;

            decimal lowerBand = Math.Min(taxablePay, monthlyNIUpperEarningsLimit) - monthlyNIPrimaryThreshold;
            if (lowerBand > 0)
                ni += lowerBand * NIStandardRate;

            if (taxablePay > monthlyNIUpperEarningsLimit)
            {
                decimal upperBand = taxablePay - monthlyNIUpperEarningsLimit;
                ni += upperBand * NIUpperRate;
            }

            return ni;
        }
    }
}
