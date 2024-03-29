﻿using System;
using Demo.Application.Shared.Dtos;

namespace Demo.Application.Invoices.Queries.SearchInvoices.Dtos;

public class SearchInvoiceDto : SoftDeleteEntityDto
{
    public string InvoiceNumber { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; }
    public DateTime InvoiceDate { get; set; }
    public int PaymentTerm { get; set; }
    public string OrderReference { get; set; }
    public InvoiceStatusEnum Status { get; set; }
}
