﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Demo.Application.Shared.Dtos;
using Demo.Domain.Auditlog;
using Demo.Domain.Invoice;
using Demo.Domain.Shared.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Demo.Application.Invoices.Queries.GetInvoiceAuditlog;

public class
    GetInvoiceAuditlogQueryHandler : IRequestHandler<GetInvoiceAuditlogQuery, GetInvoiceAuditlogQueryResult>
{
    private readonly IMapper _mapper;
    private readonly IDbQuery<Auditlog> _query;

    public GetInvoiceAuditlogQueryHandler(
        IDbQuery<Auditlog> query,
        IMapper mapper
    )
    {
        _query = query;
        _mapper = mapper;
    }

    public async Task<GetInvoiceAuditlogQueryResult> Handle(GetInvoiceAuditlogQuery request,
        CancellationToken cancellationToken)
    {
        var query = _query.AsQueryable()
            .Where(x => x.EntityName == nameof(Invoice))
            .Where(x => x.EntityId == request.InvoiceId);

        var totalItems = await query.CountAsync(cancellationToken);

        var auditLogs = await query
            .OrderByDescending(c => c.ModifiedOn)
            .Skip(request.PageSize * request.PageIndex)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new GetInvoiceAuditlogQueryResult
        {
            PageIndex = request.PageIndex,
            PageSize = request.PageSize,
            TotalItems = totalItems,
            InvoiceId = request.InvoiceId,
            Auditlogs = _mapper.Map<IEnumerable<AuditlogDto>>(auditLogs)
        };
    }
}
