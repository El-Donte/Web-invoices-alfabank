using AbsIntegrationService.Models;

namespace AbsIntegrationService.Services.DraftCalculator;

public class DraftCalculatorService : IDraftCalculatorService
{
    public void CalculateTotals(InvoiceDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        var totalWithoutNds = 0m;
        var totalNds = 0m;

        foreach (var line in draft.Lines)
        {
            CalculateLine(line);
            totalWithoutNds += line.AmountWithoutNds;
            totalNds += line.NdsAmount;
        }

        draft.TotalWithoutNds = decimal.Round(totalWithoutNds, 2, MidpointRounding.AwayFromZero);
        draft.TotalNds = decimal.Round(totalNds, 2, MidpointRounding.AwayFromZero);
        draft.TotalWithNds = decimal.Round(totalWithoutNds + totalNds, 2, MidpointRounding.AwayFromZero);

        draft.UpdatedAt = DateTime.UtcNow;
    }
    
    public InvoiceDraftLine CalculateLine(InvoiceDraftLine line)
    {
        ArgumentNullException.ThrowIfNull(line);

        line.AmountWithoutNds = decimal.Round(line.PriceWithoutNds * line.Quantity, 2, MidpointRounding.AwayFromZero);
        
        line.NdsAmount = CalculateNdsAmount(line.AmountWithoutNds, line.NdsRate);
        
        line.AmountWithNds = decimal.Round(line.AmountWithoutNds + line.NdsAmount, 2, MidpointRounding.AwayFromZero);

        return line;
    }
    
    public decimal CalculateNdsAmount(decimal amountWithoutNds, decimal ndsRate)
    {
        return ndsRate switch
        {
            <= 0 => 0m,
            22m or 12m => decimal.Round(amountWithoutNds * ndsRate / (100 + ndsRate), 2, MidpointRounding.AwayFromZero),
            _ => decimal.Round(amountWithoutNds * ndsRate / 100m, 2, MidpointRounding.AwayFromZero)
        };
    }
}