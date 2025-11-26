using ContractorTakeHomePayCalculator.Api.Configuration;
using ContractorTakeHomePayCalculator.Api.Services;

namespace ContractorTakeHomePayCalculator.Api.Tests
{
    [TestFixture]
    public class WhenCalculatingTaxFreeAllowance
    {
        private TaxCodeCalculatorService _sut;
        private TakeHomePayCalculatorConfiguration _configuration;

        [SetUp]
        public void Setup()
        {
            _configuration = new TakeHomePayCalculatorConfiguration
            {
                PersonalAllowance = 12570m
            };

            _sut = new TaxCodeCalculatorService(_configuration);
        }

        [TestCase("3000Z", 12570)]
        [TestCase("AR242", 12570)]
        [TestCase("", 12570)]
        public void ThenTheDefaultTaxFreeAllowanceIsUsedIfNoTaxCodeSuppliedOrUnrecognised(string taxCode, decimal expectedTaxFreeAllowance)
        {
            var taxFreeAllowance = _sut.CalculateTaxFreeAllowance(taxCode);

            Assert.That(taxFreeAllowance.Equals(expectedTaxFreeAllowance));
        }

        [TestCase("1257L", 12570)]
        [TestCase("356L", 3560)]
        [TestCase("544L", 5440)]
        [TestCase("223L", 2230)]
        [TestCase("700T", 7000)]
        [TestCase("1234T", 12340)]
        public void ThenAPositiveTaxFreeAllowanceIsCalculated(string taxCode, decimal expectedTaxFreeAllowance)
        {
            var taxFreeAllowance = _sut.CalculateTaxFreeAllowance(taxCode);

            Assert.That(taxFreeAllowance.Equals(expectedTaxFreeAllowance));
        }

        [TestCase("616K", -6160)]
        [TestCase("6K", -60)]
        public void ThenANegativeTaxFreeAllowanceIsCalculated(string taxCode, decimal expectedTaxFreeAllowance)
        {
            var taxFreeAllowance = _sut.CalculateTaxFreeAllowance(taxCode);

            Assert.That(taxFreeAllowance.Equals(expectedTaxFreeAllowance));
        }

        [Test]
        public void ThenAZeroTaxFreeAllowanceIsCalculated()
        {
            var taxCode = "0L";

            var taxFreeAllowance = _sut.CalculateTaxFreeAllowance(taxCode);
            
            Assert.That(taxFreeAllowance.Equals(0));
        }
    }
}
