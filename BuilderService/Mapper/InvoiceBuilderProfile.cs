using AutoMapper;
using BuilderService.Models;
using Shared.Models;

namespace BuilderService.Mapper;

internal sealed class InvoiceBuilderProfile : Profile
{
    public InvoiceBuilderProfile()
    {
        CreateMap<Invoice, InvoiceFields>()
            .ForMember(a => a.CustomerName, opt => opt.MapFrom(b => b.CustomerName))
            .ForMember(a => a.InvoiceNumber, opt => opt.MapFrom(b => b.InvoiceNumber))
            .ForMember(a => a.InvoiceDate, opt => opt.MapFrom(b => b.InvoiceDate.ToString("MMMM d, yyyy")))
            .ForMember(a => a.AddressLine1, opt => opt.MapFrom(b => b.CustomerAddressLine1))
            .ForMember(a => a.City, opt => opt.MapFrom(b => b.CustomerCity))
            .ForMember(a => a.State, opt => opt.MapFrom(b => b.CustomerState))
            .ForMember(a => a.PostalCode, opt => opt.MapFrom(b => b.CustomerPostalCode))
            .ForMember(a => a.FullDueDate, opt => opt.MapFrom(b => b.InvoiceDate.ToString("MMMM d, yyyy")))
            .ForMember(a => a.SalesTax, opt => opt.MapFrom(b => b.SalesTax.ToString("C2")))
            .ForMember(a => a.Subtotal, opt => opt.MapFrom(b => b.LineItems.Sum(c => c.Total).ToString("C2")))
            .ForMember(a => a.TotalDue,
                opt => opt.MapFrom(b => b.Total.ToString("C2")))
            .ForMember(a => a.ShortDueDate, opt => opt.MapFrom(b => b.DueDate.ToString("MM.dd.yyyy")))
            .ForMember(a => a.LineItems, opt => opt.MapFrom(b => b.LineItems));

        CreateMap<LineItem, InvoiceLineItem>()
            .ForMember(a => a.Quantity, opt => opt.MapFrom(b => b.Quantity))
            .ForMember(a => a.Description, opt => opt.MapFrom(b => b.Description))
            .ForMember(a => a.UnitPrice, opt => opt.MapFrom(b => b.UnitPrice))
            .ForMember(a => a.Total, opt => opt.MapFrom(b => b.Total.ToString("C2")));
    }
}