﻿using System;
using System.Collections.Generic;
using Demo.Common.Interfaces;
using Demo.Domain.Auditlog;
using Demo.Domain.Auditlog.Interfaces;
using Demo.Domain.Invoice;
using Demo.Domain.Shared.Interfaces;
using Demo.Infrastructure.Auditlogging.Shared;

namespace Demo.Infrastructure.Auditlogging;

internal class InvoiceAuditlogger : AuditloggerBase<Invoice>, IAuditlogger<Invoice>
{
    public InvoiceAuditlogger(
        ICurrentUserIdProvider currentUserIdProvider,
        IDateTime dateTime,
        IAuditlogDomainEntity auditlogDomainEntity
    ) : base(currentUserIdProvider, dateTime, auditlogDomainEntity)
    {
    }

    protected override List<AuditlogItem> AuditlogItems(Invoice current, Invoice previous)
    {
        return new AuditlogBuilder<Invoice>()
            .WithProperty(c => c.InvoiceNumber)
            .WithProperty(c => c.CustomerId)
            .WithProperty(c => c.InvoiceDate, AuditlogType.DateOnly)
            .WithProperty(c => c.PaymentTerm)
            .WithProperty(c => c.OrderReference)
            .WithProperty(c => c.Status, customFormatter: InvoiceStatusEnumFormatter)
            .WithChildEntityCollection(c => c.InvoiceLines, new AuditlogBuilder<InvoiceLine>()
                .WithProperty(c => c.LineNumber)
                .WithProperty(c => c.Quantity)
                .WithProperty(c => c.Description)
                .WithProperty(c => c.SellingPrice, AuditlogType.Currency)
            )
            .Build(current, previous);
    }

    private string InvoiceStatusEnumFormatter(Enum value)
    {
        var status = (InvoiceStatus)value;
        return status switch
        {
            InvoiceStatus.Draft => "Concept",
            InvoiceStatus.Sent => "Verzonden",
            InvoiceStatus.Paid => "Betaald",
            InvoiceStatus.Cancelled => "Geannuleerd",
            _ => value.ToString("G")
        };
    }
}
