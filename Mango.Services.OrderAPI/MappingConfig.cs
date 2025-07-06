using AutoMapper;
using Mango.Services.OrderAPI.Models.DTO.Cart;
using Mango.Services.OrderAPI.Models.DTO.Order;
using Mango.Services.OrderAPI.Models.Order;

namespace Mango.Services.OrderAPI
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<OrderHeaderDTO, CartHeaderDTO>()
                .ForMember(dest => dest.CartTotal, u => u.MapFrom(src => src.OrderTotal))
                .ReverseMap();

            CreateMap<CartDetailsDTO, OrderDetailsDTO>()
                .ForMember(dest => dest.ProductName, u => u.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.Price, u => u.MapFrom(src => src.Product.Price));

            CreateMap<OrderDetailsDTO, CartDetailsDTO>();

            CreateMap<OrderHeader, OrderHeaderDTO>().ReverseMap();
            CreateMap<OrderDetailsDTO, OrderDetails>().ReverseMap();
        }
    }
}
