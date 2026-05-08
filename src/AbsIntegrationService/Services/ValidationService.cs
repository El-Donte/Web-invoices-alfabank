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
            return "buyerInn or SellerInn is required";
        }

        if (string.IsNullOrWhiteSpace(msg.OperationNumber)) return "OperationNumber is required";
        if (msg.Quantity <= 0 || msg.UnitPrice <= 0) return "Quantity and Price must be > 0";
        if (string.IsNullOrWhiteSpace(msg.ProductCode) || string.IsNullOrWhiteSpace(msg.ProductName)) return "Product`s Code and Name is required";
        if (string.IsNullOrWhiteSpace(msg.Unit)) return "Unit is required";
        
        if (!msg.OperationDate.HasValue)
        {
            return "OperationDate is required";
        }

        var allowedRates = new HashSet<decimal>{20m, 22m ,10m};
        if (!allowedRates.Contains(msg.NdsRate))
            return $"Invalid NDS rate: {msg.NdsRate}. Allowed: {string.Join(", ", allowedRates)}";
        
        var expectedTotal = msg.Quantity * msg.UnitPrice;
        if (Math.Abs(expectedTotal - msg.Amount) > 0.01m)
            return $"Amount mismatch: expected {expectedTotal}, got {msg.Amount}";

        return null;
    }
}