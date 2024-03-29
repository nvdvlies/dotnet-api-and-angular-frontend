﻿using System;

namespace Demo.Events.Invoice;

public class InvoiceCancelledEvent : Event<InvoiceCancelledEvent, InvoiceCancelledEventData>
{
    public static InvoiceCancelledEvent Create(Guid createdBy, Guid correlationId, Guid id)
    {
        var data = new InvoiceCancelledEventData { CorrelationId = correlationId, Id = id };
        return new InvoiceCancelledEvent
        {
            Topic = Topics.Invoice,
            Data = data,
            Subject = $"Invoice/{data.Id}",
            DataVersion = data.EventDataVersion,
            CreatedBy = createdBy,
            CorrelationId = data.CorrelationId
        };
    }
}

public class InvoiceCancelledEventData : IEventData
{
    public Guid Id { get; set; }
    public string EventDataVersion => "1.0";
    public Guid CorrelationId { get; set; }
}
