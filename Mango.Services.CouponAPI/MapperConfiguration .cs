using AutoMapper;
using Mango.Services.CouponAPI.Models;

namespace Mango.Services.CouponAPI
{
    public class MapperConfiguration : Profile
    {
        public MapperConfiguration()
        {
            CreateMap<CouponDto, Coupon>();
            CreateMap<Coupon, CouponDto>();
        }
    }
}