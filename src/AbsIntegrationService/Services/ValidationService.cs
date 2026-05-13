using AbsIntegrationService.Metrics;
using AbsIntegrationService.Services.Interfaces;
using Shared.Contracts;

namespace AbsIntegrationService.Services;

//ToDo переделать на result pattern
public class ValidationService : IValidationService
{
    public string Validate(AbsMessage msg)
    {
        if (string.IsNullOrWhiteSpace(msg.BuyerInn) || string.IsNullOrWhiteSpace(msg.SellerInn))
        {
            var rule = "buyerInn or SellerInn is required";
            IngestionMetrics.RecordValidationError(rule);
            return rule;
        }

        if (string.IsNullOrWhiteSpace(msg.OperationNumber))
        {
            var rule = "OperationNumber is required";
            IngestionMetrics.RecordValidationError(rule);
            return rule;
        }
        if (msg.Quantity <= 0 || msg.UnitPrice <= 0)
        {
            var rule = "Quantity and Price must be > 0";
            IngestionMetrics.RecordValidationError(rule);
            return rule;
        }
        if (string.IsNullOrWhiteSpace(msg.ProductCode) || string.IsNullOrWhiteSpace(msg.ProductName))
        {
            var rule = "Product`s Code and Name is required";
            IngestionMetrics.RecordValidationError(rule);
            return rule;
        }
        if (string.IsNullOrWhiteSpace(msg.Unit))
        {
            var rule = "Unit is required";
            IngestionMetrics.RecordValidationError(rule);
            return rule;
        }
        
        if (!msg.OperationDate.HasValue)
        {
            var rule = "OperationDate is required";
            IngestionMetrics.RecordValidationError(rule);
            return rule;
        }

        var allowedRates = new HashSet<decimal>{20m, 22m ,10m};
        if (!allowedRates.Contains(msg.NdsRate))
        {
            var rule = $"Invalid NDS rate: {msg.NdsRate}. Allowed: {string.Join(", ", allowedRates)}";
            IngestionMetrics.RecordValidationError(rule);
            return rule;
        }

        var expectedTotal = msg.Quantity * msg.UnitPrice;
        if (Math.Abs(expectedTotal - msg.Amount) > 0.01m)
        {
            var rule = $"Amount mismatch: expected {expectedTotal}, got {msg.Amount}";
            IngestionMetrics.RecordValidationError(rule);
            return rule;
        }

        return null;
    }
}