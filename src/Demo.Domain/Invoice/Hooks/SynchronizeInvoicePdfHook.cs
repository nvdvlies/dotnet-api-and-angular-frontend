﻿using Demo.Common.Interfaces;
using Demo.Domain.Invoice.Interfaces;
using Demo.Domain.Invoice.Models;
using Demo.Domain.Invoice.State;
using Demo.Domain.Shared.DomainEntity;
using Demo.Domain.Shared.Exceptions;
using Demo.Domain.Shared.Interfaces;
using Demo.Events.Invoice;
using Demo.Messages.Invoice;
using System.Threading;
using System.Threading.Tasks;

namespace Demo.Domain.Invoice.Hooks
{
    internal class SynchronizeInvoicePdfHook : IBeforeCreate<Invoice>, IBeforeUpdate<Invoice>
    {
        public int Order => 99;

        private readonly IInvoiceToPdfModelMapper _invoiceToPdfModelMapper;
        private readonly ICorrelationIdProvider _correlationIdProvider;

        public SynchronizeInvoicePdfHook(IInvoiceToPdfModelMapper invoiceToPdfModelMapper, ICorrelationIdProvider correlationIdProvider)
        {
            _invoiceToPdfModelMapper = invoiceToPdfModelMapper;
            _correlationIdProvider = correlationIdProvider;
        }

        public async Task ExecuteAsync(HookType type, IDomainEntityContext<Invoice> context, CancellationToken cancellationToken)
        {
            if (type == HookType.BeforeCreate)
            {
                // An invoice always has an accompanying PDF document. We can create this by requesting synchronization via a domain event.
                context.Entity.PdfIsSynced = false;
                context.SendMessageToQueue(SynchronizeInvoicePdfMessage.Create(_correlationIdProvider.Id, context.Entity.Id));
            }
            else if (type == HookType.BeforeUpdate && context.Pristine.Status == InvoiceStatus.Draft)
            {
                var invoiceToPdfModel = await _invoiceToPdfModelMapper.MapAsync(context.Entity, cancellationToken);
                var checksum = invoiceToPdfModel.GetChecksum();

                // Compare calculated checksum against checksum when PDF was last generated
                if (!string.Equals(checksum, context.Entity.PdfChecksum))
                {
                    context.State.TryGet(InvoiceStateKeys.ThrowIfPdfIsNotSynced, out bool throwIfPdfIsNotSynced);

                    if (throwIfPdfIsNotSynced)
                    {
                        // Prevent a possible infinite loop by not allowing synchronization when in the context of
                        // updating the invoice directly after PDF synchronization has occured. In this scenario the 
                        // checksums should have been equal.
                        throw new DomainException("Expected invoice PDF to have been synced");
                    }

                    // Changes made to entity DO affect PDF content. Requesting synchronization.
                    context.Entity.PdfIsSynced = false;
                    context.SendMessageToQueue(SynchronizeInvoicePdfMessage.Create(_correlationIdProvider.Id, context.Entity.Id));
                }
                else
                {
                    // Changes made to entity DIDNT affect PDF content. Not requesting synchronization.
                    context.Entity.PdfIsSynced = true;
                }
            }

            if (context.Entity.PdfIsSynced && context.IsPropertyDirty(x => x.PdfIsSynced))
            {
                context.PublishIntegrationEvent(InvoicePdfSynchronizedEvent.Create(_correlationIdProvider.Id, context.Entity.Id));
            }
        }
    }
}