﻿using System;
using Demo.Application.Shared.Interfaces;
using MediatR;

namespace Demo.Application.Invoices.Commands.DeleteInvoice;

public class DeleteInvoiceCommand : ICommand, IRequest<Unit>
{
    internal Guid Id { get; set; }

    public void SetInvoiceId(Guid id)
    {
        Id = id;
    }
}
