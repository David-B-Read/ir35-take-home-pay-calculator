namespace ContractorTakeHomePayCalculator.Api.Models
{
    public record TakeHomePayCalculationRequest(    
        decimal DayRate,
        int DaysWorked,
        decimal MonthlyFee,
        decimal SalarySacrificePension
    );
}
