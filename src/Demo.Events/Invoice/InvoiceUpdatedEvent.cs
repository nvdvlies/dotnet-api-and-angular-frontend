﻿using System;

namespace Demo.Events.Invoice
{
    public class InvoiceUpdatedEvent : Event<InvoiceUpdatedEvent, InvoiceUpdatedEventData>
    {
        public static InvoiceUpdatedEvent Create(Guid correlationId, Guid id, Guid updatedBy)
        {
            var data = new InvoiceUpdatedEventData
            {
                CorrelationId = correlationId,
                Id = id,
                UpdatedBy = updatedBy
            };
            return new InvoiceUpdatedEvent
            {
                Topic = Topics.Invoice,
                Data = data,
                Subject = $"Invoice/{data.Id}",
                DataVersion = data.EventDataVersion,
                CorrelationId = data.CorrelationId
            };
        }
    }

    public class InvoiceUpdatedEventData : IEventData
    {
        public string EventDataVersion => "1.0";
        public Guid CorrelationId { get; set; }

        public Guid Id { get; set; }
        public Guid UpdatedBy { get; set; }
    }
}
