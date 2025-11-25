using ContractorTakeHomePayCalculator.Api.Services;

namespace ContractorTakeHomePayCalculator.Api.Tests
{
    [TestFixture]
    public class WhenCalculatingTakeHomePayBreakdown
    {
        private TakeHomePayCalculatorService _sut;
        private decimal _dayRate;
        private int _daysWorked;
        private decimal _monthlyFee;
        private decimal _salarySacrificePension;
        private string _taxCode;

        [SetUp]
        public void Setup()
        {
            _sut = new TakeHomePayCalculatorService();

            _dayRate = 300;
            _daysWorked = 20;
            _monthlyFee = 100;
            _salarySacrificePension = 0;
            _taxCode = "1257L";
        }

        [Test]
        public void Then_the_assignment_rate_will_be_calculated_based_on_day_rate_and_days_worked()
        {
            var breakdown = _sut.CalculateBreakdown(_dayRate, _daysWorked, _monthlyFee, _salarySacrificePension, _taxCode);

            Assert.That(breakdown.AssignmentRate.Equals(_daysWorked * _dayRate));
        }

        [Test]
        public void Then_the_apprenticeship_levy_will_be_calculated_excluding_the_monthly_fee()
        {
            var breakdown = _sut.CalculateBreakdown(_dayRate, _daysWorked, _monthlyFee, _salarySacrificePension, _taxCode);

            var expectedApprenticeshipLevy = ((_daysWorked * _dayRate) - _monthlyFee) * 0.005m;
            Assert.That(breakdown.ApprenticeshipLevy.Equals(expectedApprenticeshipLevy));
        }

        [Test]
        public void Then_the_employer_NI_contribution_will_be_calculated_using_the_NI_threshold_and_tax_rate()
        {
            var breakdown = _sut.CalculateBreakdown(_dayRate, _daysWorked, _monthlyFee, _salarySacrificePension, _taxCode);

            var expectedEmployersNationalInsurance = (((_daysWorked * _dayRate) - _monthlyFee) - (9100m/12)) * 0.15m;
            Assert.That(breakdown.EmployerNI.Equals(expectedEmployersNationalInsurance));
        }

        [Test]
        public void Then_the_employer_NI_contribution_will_be_zero_if_below_the_NI_threshold()
        {
            var dayRateBelowThreshold = ((9100m / 12) - 1 ) / _daysWorked;

            var breakdown = _sut.CalculateBreakdown(dayRateBelowThreshold, _daysWorked, _monthlyFee, _salarySacrificePension, _taxCode);
            Assert.That(breakdown.EmployerNI.Equals(0));
        }

        [Test]
        public void Then_the_total_employer_costs_will_include_employer_NI_and_the_apprenticeship_levy()
        {
            var breakdown = _sut.CalculateBreakdown(_dayRate, _daysWorked, _monthlyFee, _salarySacrificePension, _taxCode);

            var expectedEmployerCosts = breakdown.EmployerNI + breakdown.ApprenticeshipLevy;
            Assert.That(breakdown.TotalEmployerCosts.Equals(expectedEmployerCosts));
        }

        [Test]
        public void Then_the_taxable_pay_will_be_reduced_by_the_salary_sacrifice_pension_amount_if_opting_in()
        {
            var salarySacrificePensionValue = 500m;

            var breakdown = _sut.CalculateBreakdown(_dayRate, _daysWorked, _monthlyFee, salarySacrificePensionValue, _taxCode);

            var expectedTaxablePay = breakdown.AfterMonthlyFee - breakdown.TotalEmployerCosts - salarySacrificePensionValue;
            Assert.That(breakdown.SalarySacrificePension.Equals(salarySacrificePensionValue));
            Assert.That(breakdown.TaxablePay.Equals(expectedTaxablePay));
        }

        [Test]
        public void Then_the_income_tax_will_be_calculated_as_zero_if_taxable_income_is_below_personal_allowance()
        {
            var dayRateUnderPersonalAllowance = ((12570m / 12) / _daysWorked) - 1;

            var breakdown = _sut.CalculateBreakdown(dayRateUnderPersonalAllowance, _daysWorked, _monthlyFee, _salarySacrificePension, _taxCode);

            Assert.That(breakdown.IncomeTax.Equals(0));
        }

        [Test]
        public void Then_the_income_tax_will_be_calculated_if_it_falls_within_the_basic_tax_rate()
        {
            var dayRateWithinBasicTaxRate = ((12570m / 12) / _daysWorked) + 10;

            var breakdown = _sut.CalculateBreakdown(dayRateWithinBasicTaxRate, _daysWorked, _monthlyFee, _salarySacrificePension, _taxCode);

            var expectedBasicRateTax = (breakdown.TaxablePay - (12570m / 12) ) * 0.20m;
            Assert.That(breakdown.IncomeTax.Equals(expectedBasicRateTax));
        }

        [Test]
        public void Then_the_income_tax_will_be_calculated_if_it_falls_within_the_higher_tax_rate()
        {
            var dayRateAboveHigherTaxRate = ((50270m / 12) / _daysWorked) + 50;

            var breakdown = _sut.CalculateBreakdown(dayRateAboveHigherTaxRate, _daysWorked, _monthlyFee, _salarySacrificePension, _taxCode);

            var expectedBasicRateTax = ((50270m / 12) - (12570m / 12)) * 0.20m;
            var expectedHigherRateTax = (breakdown.TaxablePay - (50270m / 12)) * 0.40m;
            
            Assert.That(breakdown.IncomeTax.Equals(expectedBasicRateTax + expectedHigherRateTax));
        }

        [Test]
        public void Then_the_income_tax_will_be_calculated_if_it_falls_within_the_additional_higher_tax_rate()
        {
            var dayRateWithinBasicTaxRate = ((125140m / 12) / _daysWorked) + 100;

            var breakdown = _sut.CalculateBreakdown(dayRateWithinBasicTaxRate, _daysWorked, _monthlyFee, _salarySacrificePension, _taxCode);

            var expectedBasicRateTax = (50270m / 12) * 0.20m;
            var expectedAdditionalRateTax = (breakdown.TaxablePay - (125140m / 12)) * 0.45m;
            var expectedHigherRateTax = (breakdown.TaxablePay - (50270m / 12) - (breakdown.TaxablePay - (125140m / 12))) * 0.40m;

            var actualIncomeTax = Math.Round(breakdown.IncomeTax, 2);
            var expectedIncomeTax = Math.Round(expectedBasicRateTax + expectedHigherRateTax + expectedAdditionalRateTax, 2);
            Assert.That(actualIncomeTax.Equals(expectedIncomeTax));
        }
        
        [Test]
        public void Then_the_employee_NI_will_be_calculated_as_zero_if_taxable_income_is_below_NI_threshold()
        {
            var dayRateUnderNationalInsuranceThreshold = ((12568m / 12) / _daysWorked) - 1;

            var breakdown = _sut.CalculateBreakdown(dayRateUnderNationalInsuranceThreshold, _daysWorked, _monthlyFee, _salarySacrificePension, _taxCode);

            Assert.That(breakdown.EmployeeNI.Equals(0));
        }

        [Test]
        public void Then_the_employee_NI_will_be_calculated_based_on_the_lower_threshold()
        {
            var dayRateAboveNationalInsurancePrimaryThreshold = ((12568m / 12) / _daysWorked) + 10;

            var breakdown = _sut.CalculateBreakdown(dayRateAboveNationalInsurancePrimaryThreshold, _daysWorked, _monthlyFee, _salarySacrificePension, _taxCode);

            var expectedEmployeeNationalInsurance = (breakdown.TaxablePay - (12568m / 12)) * 0.08m;
            Assert.That(breakdown.EmployeeNI.Equals(expectedEmployeeNationalInsurance));
        }

        [Test]
        public void Then_the_employee_NI_will_be_calculated_based_on_the_lower_and_upper_thresholds()
        {
            var dayRateAboveNationalInsuranceSecondaryThreshold = ((50270m / 12) / _daysWorked) + 50;

            var breakdown = _sut.CalculateBreakdown(dayRateAboveNationalInsuranceSecondaryThreshold, _daysWorked, _monthlyFee, _salarySacrificePension, _taxCode);

            var taxablePayApplicableToPrimaryRate = (50270m - 12568m) / 12;
            var taxablePayApplicableToUpperRate = breakdown.TaxablePay - taxablePayApplicableToPrimaryRate - (12568m / 12);
            var expectedEmployeeNationalInsurance = (taxablePayApplicableToPrimaryRate * 0.08m) + (taxablePayApplicableToUpperRate * 0.02m);
            Assert.That(breakdown.EmployeeNI.Equals(expectedEmployeeNationalInsurance));
        }

        [Test]
        public void Then_the_take_home_pay_will_be_the_taxable_pay_after_deductions()
        {
            var breakdown = _sut.CalculateBreakdown(_dayRate, _daysWorked, _monthlyFee, _salarySacrificePension, _taxCode);

            var expectedTakeHomePay = breakdown.TaxablePay - breakdown.EmployeeNI - breakdown.IncomeTax;
            Assert.That(breakdown.NetTakeHomePay.Equals(expectedTakeHomePay));
        }
    }
}