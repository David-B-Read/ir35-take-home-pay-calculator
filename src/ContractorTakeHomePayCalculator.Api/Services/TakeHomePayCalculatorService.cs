using ContractorTakeHomePayCalculator.Api.Configuration;
using ContractorTakeHomePayCalculator.Api.Models;
using Microsoft.Extensions.Options;

namespace ContractorTakeHomePayCalculator.Api.Services
{
    public class TakeHomePayCalculatorService
    {
        private readonly TakeHomePayCalculatorConfiguration _configuration; 

        public TakeHomePayCalculatorService(IOptions<TakeHomePayCalculatorConfiguration> configuration)
        {
            _configuration = configuration.Value;
        }

        public TakeHomePayCalculationBreakdown CalculateBreakdown(
            decimal dayRate,
            int daysWorked,
            decimal monthlyFee,
            decimal salarySacrificePension,
            string taxCode)
        {
            var taxFreeAllowance = CalculateTaxFreeAllowance(taxCode);

            var breakdown = new TakeHomePayCalculationBreakdown
            {
                AssignmentRate = dayRate * daysWorked,
                MonthlyFee = monthlyFee
            };
            breakdown.AfterMonthlyFee = breakdown.AssignmentRate - monthlyFee;

            breakdown.EmployerNI = CalculateEmployerNI(breakdown.AfterMonthlyFee);
            breakdown.ApprenticeshipLevy = breakdown.AfterMonthlyFee * _configuration.ApprenticeshipLevyRate;
            breakdown.TotalEmployerCosts = breakdown.EmployerNI + breakdown.ApprenticeshipLevy;

            breakdown.EmploymentCostBase = breakdown.AfterMonthlyFee - breakdown.TotalEmployerCosts;
            if (breakdown.EmploymentCostBase < 0)
            {
                breakdown.EmploymentCostBase = 0;
            }
            
            breakdown.SalarySacrificePension = salarySacrificePension;
            breakdown.TaxablePay = Math.Max(0, breakdown.EmploymentCostBase - salarySacrificePension);

            breakdown.IncomeTax = CalculateIncomeTax(breakdown.TaxablePay, taxFreeAllowance);
            breakdown.EmployeeNI = CalculateEmployeeNI(breakdown.TaxablePay);

            breakdown.NetTakeHomePay = breakdown.TaxablePay - breakdown.IncomeTax - breakdown.EmployeeNI;

            return breakdown;
        }

        private decimal CalculateTaxFreeAllowance(string taxCode)
        {
            return new TaxCodeCalculatorService(_configuration).CalculateTaxFreeAllowance(taxCode);
        }

        private decimal CalculateEmployerNI(decimal pay)
        {
            var monthlyNIThreshold = _configuration.EmployerNIThreshold / 12;

            if (pay <= monthlyNIThreshold) return 0;
            return (pay - monthlyNIThreshold) * _configuration.EmployerNIRate;
        }

        private decimal CalculateIncomeTax(decimal taxablePay, decimal taxFreeAllowance)
        {
            decimal tax = 0;
            decimal remaining = taxablePay;

            decimal adjustedPA = taxFreeAllowance / 12;
            if (taxablePay > (100000 / 12))
                adjustedPA = Math.Max(0, (taxFreeAllowance / 12) - (taxablePay - (100000 / 12)) / 2);

            remaining -= adjustedPA;
            if (remaining <= 0) return 0;

            decimal basicBand = Math.Min(remaining, (_configuration.BasicRateLimit / 12) - adjustedPA);
            tax += basicBand * _configuration.BasicRateTax;
            remaining -= basicBand;
            if (remaining <= 0) return tax;

            decimal higherBand = Math.Min(remaining, (_configuration.HigherRateLimit - _configuration.BasicRateLimit) / 12);
            tax += higherBand * _configuration.HigherRateTax;
            remaining -= higherBand;
            if (remaining <= 0) return tax;

            tax += remaining * _configuration.AdditionalRateTax;

            return tax;
        }

        private decimal CalculateEmployeeNI(decimal taxablePay)
        {
            var monthlyNIPrimaryThreshold = _configuration.NIPrimaryThreshold / 12;
            var monthlyNIUpperEarningsLimit = _configuration.NIUpperEarningsLimit / 12; 

            if (taxablePay <= monthlyNIPrimaryThreshold)
                return 0;

            decimal ni = 0;

            decimal lowerBand = Math.Min(taxablePay, monthlyNIUpperEarningsLimit) - monthlyNIPrimaryThreshold;
            if (lowerBand > 0)
                ni += lowerBand * _configuration.NIStandardRate;

            if (taxablePay > monthlyNIUpperEarningsLimit)
            {
                decimal upperBand = taxablePay - monthlyNIUpperEarningsLimit;
                ni += upperBand * _configuration.NIUpperRate;
            }

            return ni;
        }
    }
}
