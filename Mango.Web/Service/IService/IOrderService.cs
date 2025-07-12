using Mango.Web.Models;
using Mango.Web.Models.Cart;
using Mango.Web.Models.Stripe;

namespace Mango.Web.Service.IService
{

    public interface IOrderService
    {
        // Cart related methods
        Task<ResponseDTO?> CreateOrder(CartDTO cartDto);

        // Stripe related methods
        Task<ResponseDTO?> CreateStripeSession(StripeRequestDTO stripeRequestDto);
        Task<ResponseDTO?> ValidateStripeSession(int orderHeaderId);

        // Order related methods
        Task<ResponseDTO?> GetAllOrder(string? userId);
        Task<ResponseDTO?> GetOrder(int orderId);
        Task<ResponseDTO?> UpdateOrderStatus(int orderId, string newStatus);
    }
}
