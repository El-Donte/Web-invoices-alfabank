using Shared.Entities;

namespace AbsMockProducer;

public class InvoiceTestCreatedMessage
{
    public Guid MessageId { get; set; } = Guid.NewGuid();

    // Основные идентификаторы операции
    public string OperationNumber { get; set; } = "120"; 
    public DateTime OperationDate { get; set; } =  DateTime.UtcNow;
    public TransactionType Type { get; set; } = TransactionType.Shipment;

    // Суммы
    public decimal UnitPrice { get; set; } = 3_000m;
    public decimal NdsRate { get; set; } = 22m;

    public decimal NdsAmount { get; set; } = 12_000m;
    public decimal Amount { get; set; } = 60_000m;
    public string CurrencyCode { get; set; } = "RUB";

    // Покупатель
    public string BuyerInn { get; set; } = "7707083893";
    public string BuyerKpp { get; set; } = "770701001";
    public string BuyerName { get; set; } ="ООО \"Крутая компания\"";
    public string BuyerAddress { get; set; } = "ул Пушкина д. Колотушкина 22";

    // Продавец
    public string SellerInn { get; set; } = "11111111";
    public string SellerKpp { get; set; } = "1111";
    public string SellerName { get; set; } = "Покупатель";
    public string SellerAddress { get; set; } = "ул Пушкина д. Колотушкина";

    //Услуга
    public string ProductCode { get; set; } = "741";
    public string ProductName { get; set; } = "Стул";
    public string Unit { get; set; } = "шт";
    public decimal Quantity { get; set; } = 20m;

    // Дополнительная информация
    public string ContractNumber { get; set; } = "1432";
    public DateTime? ContractDate { get; set; } = DateTime.UtcNow;
    public string PaymentDocumentNumber { get; set; } = "123124";
    public string OperationType { get; set; } = "shipments";

    public Guid DepartmentId { get; set; } = new ("145b5db8-049a-486f-95ba-b2402bc5e844");

    // Метаданные
    public DateTime PostedAt { get; set; } = DateTime.UtcNow;
}