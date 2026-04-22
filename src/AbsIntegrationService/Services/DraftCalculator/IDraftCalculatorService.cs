using AbsIntegrationService.Models;

namespace AbsIntegrationService.Services.DraftCalculator;

public interface IDraftCalculatorService
{
    void CalculateTotals(InvoiceDraft draft);
    InvoiceDraftLine CalculateLine(InvoiceDraftLine line);
    decimal CalculateNdsAmount(decimal amountWithoutNds, decimal ndsRate);
}