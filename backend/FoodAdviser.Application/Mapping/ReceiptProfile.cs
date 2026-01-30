using AutoMapper;
using FoodAdviser.Application.DTOs.Receipts;
using FoodAdviser.Domain.Entities;

namespace FoodAdviser.Application.Mapping;

/// <summary>
/// AutoMapper profile for Receipt mappings.
/// </summary>
public class ReceiptProfile : Profile
{
    public ReceiptProfile()
    {
        CreateMap<ReceiptLineItem, ReceiptLineItemDto>();

        CreateMap<Receipt, ReceiptDto>()
            .ForMember(d => d.Items, o => o.MapFrom(s => s.Items))
            .ForMember(d => d.Total, o => o.MapFrom(s => s.Items.Sum(i => i.Price)));
    }
}
