using Mango.Web.Models;
using Mango.Web.Models.Coupon;

namespace Mango.Web.Service.IService
{
    public interface IBaseService
    {
        Task<ResponseDTO?> SendAsync(RequestDto requestDto, bool withBearer = true);
    }
}
