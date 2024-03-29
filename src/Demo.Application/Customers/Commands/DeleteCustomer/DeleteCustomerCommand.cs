﻿using System;
using Demo.Application.Shared.Interfaces;
using MediatR;

namespace Demo.Application.Customers.Commands.DeleteCustomer;

public class DeleteCustomerCommand : ICommand, IRequest<Unit>
{
    internal Guid Id { get; set; }

    public void SetCustomerId(Guid id)
    {
        Id = id;
    }
}
