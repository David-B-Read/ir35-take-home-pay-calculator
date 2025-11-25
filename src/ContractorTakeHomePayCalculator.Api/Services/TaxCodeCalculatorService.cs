namespace ContractorTakeHomePayCalculator.Api.Services
{
    public class TaxCodeCalculatorService
    {
        private const decimal DefaultPersonalAllowance = 12570m;

        public decimal CalculateTaxFreeAllowance(string taxCode)
        {
            if (String.IsNullOrWhiteSpace(taxCode))
            {
                return DefaultPersonalAllowance;
            }

            if (taxCode.ToUpper().EndsWith("K"))
            {
                return -10 * ParseTaxNumericPortionOfTaxCode(taxCode);
            }

            if (taxCode.ToUpper().EndsWith("L") || taxCode.ToUpper().EndsWith("T"))
            {
                return 10 * ParseTaxNumericPortionOfTaxCode(taxCode);
            }

            return DefaultPersonalAllowance;
        }

        private decimal ParseTaxNumericPortionOfTaxCode(string taxCode)
        {
            var numericValue = taxCode.Substring(0, taxCode.Length - 1);
            var parsed = decimal.TryParse(numericValue, out var convertedValue);
            if (!parsed)
            {
                return DefaultPersonalAllowance;
            }

            return convertedValue;
        }
    }
}
