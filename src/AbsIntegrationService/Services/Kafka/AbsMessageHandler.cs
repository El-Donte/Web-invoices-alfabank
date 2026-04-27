using AbsIntegrationService.Contracts;
using AbsIntegrationService.Infrastructure.Repositories;
using AbsIntegrationService.Models;
using AbsIntegrationService.Models.Enums;
using AbsIntegrationService.Services.DraftCalculator;
using Messaging.Kafka.Consumer;

namespace AbsIntegrationService.Services.Kafka;

public class AbsMessageHandler(IDraftCalculatorService calculator, IInvoiceDraftRepository repository, IInvoiceDraftCreatedProducer producer) : IMessageHandler<AbsMessage>
{
    public async Task HandleAsync(AbsMessage msg, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(msg.OperationNumber))
        {
            return;
        }

        InvoiceDraft? draft;
        if (await repository.ExistsAsync(msg.OperationNumber, ct))
        {
            Console.WriteLine($"Found existing operation number {msg.OperationNumber}");
            draft = await repository.GetDraftByOperationNumberAsync(msg.OperationNumber, ct);
            Console.WriteLine($"Found draft with id {draft.Id} was created");
        }
        else
        {
            Console.WriteLine($"Creating new operation number {msg.OperationNumber}");
            draft = await CreateNewDraftFromMessage(msg, ct);
        }

        if (draft == null)
        {
            return;
        }
        
        draft.Lines.Add(await CreateLineFromMessage(draft.Id, msg, ct));
        draft.LinkedOperations.Add(await CreateOperationLink(draft.Id, msg, ct));
        
        calculator.CalculateTotals(draft);
        
        await repository.UpdateDraftAsync(draft, ct);

        if (ShouldPublishDraft(draft))
        {
            draft.Status = DraftStatus.Ready;
            await repository.MarkAsReadyAsync(draft.Id, ct);

            var draftEvent = MapToDraftCreatedEvent(draft);
            await producer.ProduceDraftCreatedAsync(draftEvent, ct);
        }
    }

    private bool ShouldPublishDraft(InvoiceDraft draft)
    {
        // return draft.Lines.Count > 0 && draft.TotalWithNds > 0;
        return false;
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

    private async Task<InvoiceDraftLine> CreateLineFromMessage(Guid draftId, AbsMessage msg, CancellationToken ct)
    {
        var draftLine = new InvoiceDraftLine(
            draftId, msg.ServiceCode, msg.ServiceName, 
            msg.Quantity, msg.Unit, msg.NdsRate, 
            msg.PriceWithoutNds, msg.ContractNumber
        );
        
        draftLine = calculator.CalculateLine(draftLine);
        
        await repository.AddDraftLineAsync(draftId, draftLine, ct);

        return draftLine;
    }

    private async Task<InvoiceDraft> CreateNewDraftFromMessage(AbsMessage msg, CancellationToken ct)
    {
        var draft = new InvoiceDraft(
            msg.OperationNumber, msg.OperationDate, msg.SellerInn, 
            msg.SellerKpp, msg.SellerName, msg.SellerAddress,
            msg.BuyerInn, msg.BuyerKpp, msg.BuyerName, 
            msg.BuyerAddress, msg.CurrencyCode
        );
        
        await repository.AddDraftAsync(draft, ct);
        
        return draft;
    }
    
    private async Task<DraftOperationLink> CreateOperationLink(Guid draftId, AbsMessage msg, CancellationToken ct)
    {
        var operationLink = new DraftOperationLink
        {
            InvoiceDraftId = draftId,
            OperationNumber = msg.OperationNumber,
            OperationDate = msg.OperationDate,
            Amount = msg.PriceWithoutNds,
            SourceMessageId = msg.MessageId.ToString()
        };
        
        await repository.AddOperationLinkAsync(draftId, operationLink, ct);
        return operationLink;
    }
}