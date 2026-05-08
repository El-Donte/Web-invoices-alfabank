using Shared.Entities;

namespace AbsIntegrationService.Services.DraftCalculator;

public interface IDraftCalculatorService
{
    void CalculateTotals(DraftInvoice draft);
    DraftInvoiceLine CalculateLine(DraftInvoiceLine line);
    decimal CalculateNdsAmount(decimal amountWithoutNds, decimal ndsRate);
}