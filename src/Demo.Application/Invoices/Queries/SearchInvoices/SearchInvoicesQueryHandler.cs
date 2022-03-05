using AutoMapper;
using AutoMapper.QueryableExtensions;
using Demo.Application.Invoices.Queries.SearchInvoices.Dtos;
using Demo.Application.Shared.Extensions;
using Demo.Application.Shared.Models;
using Demo.Domain.Invoice;
using Demo.Domain.Shared.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Demo.Application.Invoices.Queries.SearchInvoices
{
    public class SearchInvoicesQueryHandler : IRequestHandler<SearchInvoicesQuery, SearchInvoicesQueryResult>
    {
        private readonly IDbQuery<Invoice> _query;
        private readonly IMapper _mapper;

        public SearchInvoicesQueryHandler(
            IDbQuery<Invoice> query,
            IMapper mapper
        )
        {
            _query = query;
            _mapper = mapper;
        }

        public async Task<SearchInvoicesQueryResult> Handle(SearchInvoicesQuery request, CancellationToken cancellationToken)
        {
            var query = _query.AsQueryable()
                .Include(x => x.Customer)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.InvoiceNumber))
            {
                query = query.Where(x => EF.Functions.Like(x.InvoiceNumber, $"%{request.InvoiceNumber}%"));
            }

            var totalItems = await query.CountAsync(cancellationToken);

            var sortOrder = request.OrderByDescending ? SortDirection.Descending : SortDirection.Ascending;

            query = request.OrderBy switch
            {
                SearchInvoicesOrderByEnum.InvoiceNumber => query.OrderBy(x => x.InvoiceNumber, sortOrder),
                SearchInvoicesOrderByEnum.InvoiceDate => query.OrderBy(x => x.InvoiceDate, sortOrder),
                _ => throw new Exception($"OrderBy '{request.OrderBy}' not implemented.")
            };

            var invoices = await query
                .Skip(request.PageSize * request.PageIndex)
                .Take(request.PageSize)
                .ProjectTo<SearchInvoiceDto>(_mapper.ConfigurationProvider)
                //.WriteQueryStringToOutputWindowIfInDebugMode()
                .ToListAsync(cancellationToken);

            return new SearchInvoicesQueryResult
            {
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalItems = totalItems,
                Invoices = invoices
            };
        }
    }
}