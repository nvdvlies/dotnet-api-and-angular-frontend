﻿using System;
using Demo.Application.Shared.Interfaces;
using MediatR;

namespace Demo.Application.Customers.Queries.GetCustomerById;

public class GetCustomerByIdQuery : IQuery, IRequest<GetCustomerByIdQueryResult>
{
    public Guid Id { get; set; }
}
