using ContractorTakeHomePayCalculator.Api.Models;
using ContractorTakeHomePayCalculator.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TakeHomePayCalculatorService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/api/payslip",
    (TakeHomePayCalculationRequest request, TakeHomePayCalculatorService calculator) =>
    {
        var breakdown = calculator.CalculateBreakdown(
            request.DayRate,
            request.DaysWorked,
            request.MonthlyFee,
            request.SalarySacrificePension,
            request.TaxCode
        );

        return Results.Ok(breakdown);
    });

app.Run();

