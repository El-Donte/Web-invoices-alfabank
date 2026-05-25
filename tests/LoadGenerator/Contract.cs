namespace LoadGenerator;

//contarct
public class AbsMessage
{
    public Guid MessageId { get; set; }

    public string OperationNumber { get; set; }
    public DateTime? OperationDate { get; set; }
    public TransactionType Type { get; set; }

    // Суммы
    public decimal UnitPrice { get; set; }
    public decimal NdsRate { get; set; } = 22m;
    
    public decimal NdsAmount { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; }

    // Покупатель
    public string BuyerInn { get; set; }
    public string BuyerKpp { get; set; }
    public string BuyerName { get; set; }
    public string BuyerAddress { get; set; }

    // Продавец
    public string SellerInn { get; set; }
    public string SellerKpp { get; set; }
    public string SellerName { get; set; }
    public string SellerAddress { get; set; }

    //Услуга
    public string ProductCode { get; set; }
    public string ProductName { get; set; }
    public string Unit { get; set; } = "шт";
    public decimal Quantity { get; set; } = 1m;

    // Дополнительная информация
    public string ContractNumber { get; set; } = string.Empty;
    public DateTime? ContractDate { get; set; }
    public string PaymentDocumentNumber { get; set; } = string.Empty;
    public DateTime? PaymentDocumentDate { get; set; }
    public string OperationType { get; set; } = string.Empty;

    public Guid DepartmentId { get; set; }
    
    // Метаданные
    public DateTime PostedAt { get; set; } = DateTime.UtcNow;
}

public enum TransactionType
{
    Shipment = 0,
    Advance = 1,
    Corrective = 2
}