using ContractorTakeHomePayCalculator.Api.Configuration;

namespace ContractorTakeHomePayCalculator.Api.Services
{
    public class TaxCodeCalculatorService
    {
        private readonly TakeHomePayCalculatorConfiguration _configuration;

        public TaxCodeCalculatorService(TakeHomePayCalculatorConfiguration configuration)
        {
            _configuration = configuration;
        }

        public decimal CalculateTaxFreeAllowance(string taxCode)
        {
            if (String.IsNullOrWhiteSpace(taxCode))
            {
                return _configuration.PersonalAllowance;
            }

            if (taxCode.ToUpper().EndsWith("K"))
            {
                return -10 * ParseTaxNumericPortionOfTaxCode(taxCode);
            }

            if (taxCode.ToUpper().EndsWith("L") || taxCode.ToUpper().EndsWith("T"))
            {
                return 10 * ParseTaxNumericPortionOfTaxCode(taxCode);
            }

            return _configuration.PersonalAllowance;
        }

        private decimal ParseTaxNumericPortionOfTaxCode(string taxCode)
        {
            var numericValue = taxCode.Substring(0, taxCode.Length - 1);
            var parsed = decimal.TryParse(numericValue, out var convertedValue);
            if (!parsed)
            {
                return _configuration.PersonalAllowance;
            }

            return convertedValue;
        }
    }
}
