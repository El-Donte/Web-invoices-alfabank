namespace InvoicesWebService.Services.Kafka;

public class InvoiceTestCreatedMessage
{
    public Guid MessageId { get; set; } = Guid.NewGuid();

    // Основные идентификаторы операции
    public string OperationNumber { get; set; } = "100"; 
    public DateTime OperationDate { get; set; } =  DateTime.UtcNow;

    // Суммы
    public decimal PriceWithoutNds { get; set; } = 30000m;
    public decimal NdsRate { get; set; } = 22m;
    public string CurrencyCode { get; set; } = "60";

    // Покупатель
    public string BuyerInn { get; set; } = "111";
    public string BuyerKpp { get; set; } = "1111";
    public string BuyerName { get; set; } = "Покупатель";
    public string BuyerAddress { get; set; } = "ул Пушкина д. Колотушкина";

    // Продавец
    public string SellerInn { get; set; } = "7707083893";
    public string SellerKpp { get; set; } = "770701001";
    public string SellerName { get; set; } = "ООО \"Крутая компания\"";
    public string SellerAddress { get; set; } = "ул Пушкина д. Колотушкина 22";

    //Услуга
    public string ServiceCode { get; set; } = "741";
    public string ServiceName { get; set; } = "Стул";
    public string Unit { get; set; } = "шт";
    public decimal Quantity { get; set; } = 20m;

    // Дополнительная информация
    public string ContractNumber { get; set; } = "1432";
    public DateTime? ContractDate { get; set; } = DateTime.UtcNow;
    public string PaymentDocumentNumber { get; set; } = "123124";
    public string OperationType { get; set; } = "shipments";

    // Метаданные
    public DateTime PostedAt { get; set; } = DateTime.UtcNow;
}