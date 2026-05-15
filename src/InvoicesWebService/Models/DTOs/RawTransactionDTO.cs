namespace InvoicesWebService.Models.DTOs;

public record RawTransactionDTO(
    Guid Id, string ProductName,string ProductCode ,
    string UnitMeasure, decimal Quantity, decimal UnitPrice, 
    decimal Amount, decimal NdsAmount, Guid CounterpartyId, 
    Guid DepartmentId, string CurrencyCode, decimal NdsRate);