﻿using Demo.Domain.Shared.Interfaces;
using System;

namespace Demo.Domain.Invoice.DomainEntity.Events
{
    public class InvoiceDeletedDomainEvent : IDomainEvent
    {
        public Guid Id { get; set; }
        public Guid DeletedBy { get; set; }

        public InvoiceDeletedDomainEvent(Guid id, Guid deletedBy)
        {
            Id = id;
            DeletedBy = deletedBy;
        }
    }
}