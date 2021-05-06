﻿using Demo.Application.Shared.Dtos;
using System;
using System.Collections.Generic;

namespace Demo.Application.Invoices.Queries.GetInvoiceById.Dtos
{
    public class InvoiceDto : SoftDeleteEntityDto
    {
        public string InvoiceNumber { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public int PaymentTerm { get; set; }
        public string OrderReference { get; set; }
        public InvoiceStatusEnum Status { get; set; }
        public bool PdfIsSynced { get; set; }
        public int PdfHashcode { get; set; }
        public List<InvoiceLineDto> InvoiceLines { get; set; }
    }
}
