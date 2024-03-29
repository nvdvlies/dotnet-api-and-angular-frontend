﻿using System.Collections.Generic;
using AutoMapper;
using Demo.Application.Invoices.Commands.UpdateInvoice.Dtos;
using Demo.Application.Shared.Mappings;
using Demo.Domain.Invoice;

namespace Demo.Application.Invoices.Commands.UpdateInvoice;

public class UpdateInvoiceMappingProfile : Profile
{
    public UpdateInvoiceMappingProfile()
    {
        CreateMap<UpdateInvoiceCommandInvoiceLineDto, InvoiceLine>()
            .ForMember(x => x.LineNumber, opt => opt.Ignore())
            .ForMember(x => x.InvoiceId, opt => opt.Ignore())
            .ForMember(x => x.Invoice, opt => opt.Ignore())
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.xmin, opt => opt.Ignore());

        CreateMap<UpdateInvoiceCommand, Invoice>()
            .ForMember(dest => dest.InvoiceLines,
                opt => opt
                    .MapFrom<TrackedChildCollectionValueResolver<UpdateInvoiceCommand, Invoice,
                        UpdateInvoiceCommandInvoiceLineDto, InvoiceLine>, List<UpdateInvoiceCommandInvoiceLineDto>>(
                        src => src.InvoiceLines)
            )
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.InvoiceNumber, opt => opt.Ignore())
            .ForMember(x => x.Status, opt => opt.Ignore())
            .ForMember(x => x.Customer, opt => opt.Ignore())
            .ForMember(x => x.PdfIsSynced, opt => opt.Ignore())
            .ForMember(x => x.PdfChecksum, opt => opt.Ignore())
            .ForMember(x => x.Deleted, opt => opt.Ignore())
            .ForMember(x => x.DeletedBy, opt => opt.Ignore())
            .ForMember(x => x.DeletedOn, opt => opt.Ignore())
            .ForMember(x => x.CreatedBy, opt => opt.Ignore())
            .ForMember(x => x.CreatedOn, opt => opt.Ignore())
            .ForMember(x => x.LastModifiedBy, opt => opt.Ignore())
            .ForMember(x => x.LastModifiedOn, opt => opt.Ignore());
    }
}
