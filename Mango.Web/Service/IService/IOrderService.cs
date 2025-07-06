using Mango.Web.Models;
using Mango.Web.Models.Cart;
using Mango.Web.Models.Stripe;

namespace Mango.Web.Service.IService
{

    public interface IOrderService
    {
        Task<ResponseDTO?> CreateOrder(CartDTO cartDto);
        Task<ResponseDTO?> CreateStripeSession(StripeRequestDTO stripeRequestDto);
        Task<ResponseDTO?> ValidateStripeSession(int orderHeaderId);
    }
}
