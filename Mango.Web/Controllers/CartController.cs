using Mango.Web.Models;
using Mango.Web.Models.Cart;
using Mango.Web.Models.Order;
using Mango.Web.Models.Stripe;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace Mango.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        public CartController(ICartService cartService, IOrderService orderService)
        {
            _cartService = cartService;
            _orderService = orderService;
        }

        [Authorize]
        public async Task<IActionResult> CartIndex()
        {
            return View(await LoadCartDtoBasedOnLoggedInUser());
        }

        public async Task<IActionResult> Remove(int cartDetailsId)
        {
            var userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
            ResponseDTO? response = await _cartService.RemoveFromCartAsync(cartDetailsId);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Cart updated successfully";
                return RedirectToAction(nameof(CartIndex));
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(CartDTO cartDto)
        {

            ResponseDTO? response = await _cartService.ApplyCouponAsync(cartDto);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Cart updated successfully";
                return RedirectToAction(nameof(CartIndex));
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCoupon(CartDTO cartDto)
        {
            cartDto.CartHeader.CouponCode = "";
            ResponseDTO? response = await _cartService.ApplyCouponAsync(cartDto);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Cart updated successfully";
                return RedirectToAction(nameof(CartIndex));
            }
            return View();
        }

        private async Task<CartDTO> LoadCartDtoBasedOnLoggedInUser()
        {
            var userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                // Optionally, handle the case where userId is null or empty (e.g., log, throw, or return empty cart)
                return new CartDTO();
            }
            ResponseDTO? response = await _cartService.GetCartByUserIdAsync(userId);
            if (response != null && response.IsSuccess && response.Data != null)
            {
                var cartDto = JsonConvert.DeserializeObject<CartDTO>(Convert.ToString(response.Data) ?? string.Empty);
                return cartDto ?? new CartDTO();
            }
            return new CartDTO();
        }

        [HttpPost]
        public async Task<IActionResult> EmailCart(CartDTO cartDto)
        {
            // Load the cart for the logged-in user
            CartDTO cart = await LoadCartDtoBasedOnLoggedInUser();

            // Set the email from the logged-in user's claims
            cart.CartHeader.Email = User.Claims
                .Where(c => c.Type == JwtRegisteredClaimNames.Email)
                .FirstOrDefault()?.Value;

            // Ensure the email is not null or empty before proceeding
            if (string.IsNullOrEmpty(cart.CartHeader.Email))
            {
                TempData["error"] = "Email address is missing.";
                return RedirectToAction(nameof(CartIndex));
            }

            // Call the EmailCart service method
            ResponseDTO? response = await _cartService.EmailCart(cart);

            // Check the response and handle success or failure
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Email will be processed and sent shortly.";
                return RedirectToAction(nameof(CartIndex));
            }

            TempData["error"] = "Failed to send the email.";
            return RedirectToAction(nameof(CartIndex));
        }

        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            return View(await LoadCartDtoBasedOnLoggedInUser());
        }


        [HttpPost]
        [ActionName("Checkout")]
        public async Task<IActionResult> Checkout(CartDTO cartDto)
        {

            CartDTO cart = await LoadCartDtoBasedOnLoggedInUser();
            cart.CartHeader.Phone = cartDto.CartHeader.Phone;
            cart.CartHeader.Email = cartDto.CartHeader.Email;
            cart.CartHeader.Name = cartDto.CartHeader.Name;

            var response = await _orderService.CreateOrder(cart);
            OrderHeaderDTO orderHeaderDto = JsonConvert.DeserializeObject<OrderHeaderDTO>(Convert.ToString(response.Data));

            if (response != null && response.IsSuccess)
            {
                //get stripe session and redirect to stripe to place order
                //
                var domain = Request.Scheme + "://" + Request.Host.Value + "/";

                StripeRequestDTO stripeRequestDto = new()
                {
                    ApprovedUrl = domain + "cart/Confirmation?orderId=" + orderHeaderDto.OrderHeaderId,
                    CancelUrl = domain + "cart/checkout",
                    OrderHeader = orderHeaderDto
                };

                var stripeResponse = await _orderService.CreateStripeSession(stripeRequestDto);
                StripeRequestDTO stripeResponseResult = JsonConvert.DeserializeObject<StripeRequestDTO>
                                            (Convert.ToString(stripeResponse.Data));
                Response.Headers.Add("Location", stripeResponseResult.StripeSessionUrl);
                return new StatusCodeResult(303);

            }
            return View();
        }


        public async Task<IActionResult> Confirmation(int orderId)
        {
            ResponseDTO? response = await _orderService.ValidateStripeSession(orderId);
            if (response != null & response.IsSuccess)
            {

                OrderHeaderDTO orderHeader = JsonConvert.DeserializeObject<OrderHeaderDTO>(Convert.ToString(response.Data));
                if (orderHeader.Status == SD.Status_Approved)
                {
                    return View(orderId);
                }
            }
            //redirect to some error page based on status
            return View(orderId);
        }
    }
}
