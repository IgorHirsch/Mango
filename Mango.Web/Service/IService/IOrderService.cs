using Mango.Web.Models;
using Mango.Web.Models.Cart;

namespace Mango.Web.Service.IService
{

    public interface IOrderService
    {
        Task<ResponseDTO?> CreateOrder(CartDTO cartDto);
    }
}
