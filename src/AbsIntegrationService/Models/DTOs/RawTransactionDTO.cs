using Shared.Entities;

namespace AbsIntegrationService.Models.DTOs;

public class RawTransactionDTO
{
    public Guid Id {get; set;} 
    public string OperationNumber {get; set;}
    public TransactionType Type { get; set; }
    public decimal Amount {get; set;}
    public decimal NdsAmount { get; set; }
}