using AbsIntegrationService.Contracts;
using AbsIntegrationService.Infrastructure.Repositories;
using AbsIntegrationService.Models;
using AbsIntegrationService.Models.Enums;
using AbsIntegrationService.Services.DraftCalculator;
using Messaging.Kafka.Consumer;

namespace AbsIntegrationService.Services.Kafka;

public class AbsMessageHandler(IDraftCalculatorService calculator, IInvoiceDraftRepository repository, IInvoiceDraftCreatedProducer producer) : IMessageHandler<AbsMessage>
{
    public async Task HandleAsync(AbsMessage msg, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(msg.OperationNumber))
        {
            return;
        }
        
        var draft = await repository.GetDraftByOperationNumberAsync(msg.OperationNumber) ?? CreateNewDraftFromMessage(msg);

        var line = CreateLineFromMessage(msg, draft.Id);
        draft.Lines.Add(line);
        
        draft.LinkedOperations.Add(CreateOperationLink(msg, draft.Id));
        
        calculator.CalculateTotals(draft);

        await repository.SaveDraftAsync(draft);

        if (ShouldPublishDraft(draft))
        {
            draft.Status = DraftStatus.Ready;
            await repository.MarkAsReadyAsync(draft.Id);

            var draftEvent = MapToDraftCreatedEvent(draft);
            await producer.ProduceDraftCreatedAsync(draftEvent, cancellationToken);
        }
    }

    private bool ShouldPublishDraft(InvoiceDraft draft)
    {
        return draft.Lines.Count > 0 && draft.TotalWithNds > 0;
    }
    
    private InvoiceDraftCreatedEvent MapToDraftCreatedEvent(InvoiceDraft draft)
    {
        return new InvoiceDraftCreatedEvent
        {
            DraftId = draft.Id,
            OperationNumber = draft.OperationNumber,
            CreatedAt = DateTime.UtcNow,
            BuyerInn = draft.BuyerInn,
            BuyerName = draft.BuyerName,
            TotalWithNds = draft.TotalWithNds,
            Lines = draft.Lines.ToList(),
            LinkedOperationsCount = draft.LinkedOperations.Count
        };
    }
    

    private InvoiceDraftLine CreateLineFromMessage(AbsMessage msg, Guid invoiceId)
    {
        var draftLine = new InvoiceDraftLine(
            invoiceId, msg.ServiceCode, msg.ServiceName, 
            msg.Quantity, msg.Unit, msg.NdsRate, 
            msg.PriceWithoutNds, msg.ContractNumber, msg.OperationType
        );
        
        return calculator.CalculateLine(draftLine);
    }

    private InvoiceDraft CreateNewDraftFromMessage(AbsMessage msg)
    {
        return new InvoiceDraft(
                msg.OperationNumber, msg.OperationDate, msg.SellerInn, 
                msg.SellerKpp, msg.SellerName, msg.SellerAddress,
                msg.BuyerInn, msg.BuyerKpp, msg.BuyerName, 
                msg.BuyerAddress, msg.CurrencyCode
        );
    }
    
    private DraftOperationLink CreateOperationLink(AbsMessage msg, Guid draftId)
    {
        return new DraftOperationLink
        {
            InvoiceDraftId = draftId,
            OperationNumber = msg.OperationNumber,
            OperationDate = msg.OperationDate,
            Amount = msg.PriceWithoutNds,
            SourceMessageId = msg.MessageId.ToString()
        };
    }
}