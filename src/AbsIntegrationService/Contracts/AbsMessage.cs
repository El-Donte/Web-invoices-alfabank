namespace AbsIntegrationService.Contracts;

public class AbsMessage
{
    public Guid MessageId { get; set; } = Guid.NewGuid();

    // Основные идентификаторы операции
    public string OperationNumber { get; set; } = string.Empty; 
    public DateTime OperationDate { get; set; }

    // Суммы
    public decimal PriceWithoutNds { get; set; }
    public decimal NdsRate { get; set; } = 22m;
    public string CurrencyCode { get; set; } = string.Empty;

    // Покупатель
    public string BuyerInn { get; set; } = string.Empty;
    public string BuyerKpp { get; set; } = string.Empty;
    public string BuyerName { get; set; } = string.Empty;
    public string BuyerAddress { get; set; } = string.Empty;

    // Продавец
    public string SellerInn { get; set; } = "7707083893";
    public string SellerKpp { get; set; } = "770701001";
    public string SellerName { get; set; } = "ООО \"Крутая компания\"";
    public string SellerAddress { get; set; } = string.Empty;

    //Услуга
    public string ServiceCode { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string Unit { get; set; } = "шт";
    public decimal Quantity { get; set; } = 1m;

    // Дополнительная информация
    public string ContractNumber { get; set; } = string.Empty;
    public DateTime? ContractDate { get; set; }
    public string PaymentDocumentNumber { get; set; } = string.Empty;
    public string OperationType { get; set; } = string.Empty;

    // Метаданные
    public DateTime PostedAt { get; set; } = DateTime.UtcNow;
}