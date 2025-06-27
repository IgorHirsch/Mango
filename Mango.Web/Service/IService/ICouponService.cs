using Mango.Web.Models.Coupon;

namespace Mango.Web.Service.IService
{
    public interface ICouponService
    {
        Task<ResponseDTO?> GetCouponAsync(string couponCode);
        Task<ResponseDTO?> GetAllCouponsAsync();
        Task<ResponseDTO?> GetCouponByIdAsync(int id);
        Task<ResponseDTO?> CreateCouponsAsync(CouponDto couponDto);
        Task<ResponseDTO?> UpdateCouponsAsync(CouponDto couponDto);
        Task<ResponseDTO?> DeleteCouponsAsync(int id);
    }
}
