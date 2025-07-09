using AutoMapper;
using Mango.MessageBus;
using Mango.Services.ShoppingCartAPI.Data;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.DTO;
using Mango.Services.ShoppingCartAPI.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.ShoppingCartAPI.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class CartAPIController : ControllerBase
    {
        private ResponseDTO _response;
        private IMapper _mapper;
        private readonly AppDbContext _db;
        private IProductService _productService;
        private ICouponService _couponService;
        private IConfiguration _configuration;
        private readonly IMessageBus _messageBus;


        public CartAPIController(AppDbContext db, IMapper mapper, IProductService productService, ICouponService couponService, IMessageBus messageBus, IConfiguration configuration)
        {
            _db = db;
            this._response = new ResponseDTO();
            _mapper = mapper;
            _productService = productService;
            _couponService = couponService;
            _configuration = configuration;
            _messageBus = messageBus;
        }


        [HttpGet("GetCart/{userId}")]
        public async Task<ResponseDTO> GetCart(string userId)
        {
            try
            {
                CartDTO cart = new()
                {
                    CartHeader = _mapper.Map<CartHeaderDTO>(_db.CartHeaders.First(u => u.UserId == userId)),

                };
                cart.CartDetails = _mapper.Map<IEnumerable<CartDetailsDTO>>(_db.CartDetails
                    .Where(u => u.CartHeaderId == cart.CartHeader.CartHeaderId));

                IEnumerable<ProductDTO> productDtos = await _productService.GetProducts();

                foreach (var item in cart.CartDetails)
                {
                    item.Product = productDtos.FirstOrDefault(u => u.ProductId == item.ProductId);
                    if (item.Product != null)
                    {
                        cart.CartHeader.CartTotal += (item.Count * item.Product.Price);
                    }
                }

                //apply coupon if any
                if (!string.IsNullOrEmpty(cart.CartHeader.CouponCode))
                {
                    CouponDTO coupon = await _couponService.GetCoupon(cart.CartHeader.CouponCode);
                    if (coupon != null && cart.CartHeader.CartTotal > coupon.MinAmount)
                    {
                        cart.CartHeader.CartTotal -= coupon.DiscountAmount;
                        cart.CartHeader.Discount = coupon.DiscountAmount;
                    }
                }

                _response.Data = cart;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpPost("ApplyCoupon")]
        public async Task<object> ApplyCoupon([FromBody] CartDTO cartDto)
        {
            try
            {
                if (cartDto.CartHeader == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "CartHeader is null.";
                    return _response;
                }
                var cartFromDb = await _db.CartHeaders.FirstAsync(u => u.UserId == cartDto.CartHeader.UserId);
                cartFromDb.CouponCode = cartDto.CartHeader.CouponCode;
                _db.CartHeaders.Update(cartFromDb);
                await _db.SaveChangesAsync();
                _response.Data = true;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.ToString();
            }
            return _response;
        }

        [HttpPost("RemoveCoupon")]
        public async Task<object> RemoveCoupon([FromBody] CartDTO cartDto)
        {
            try
            {
                if (cartDto.CartHeader == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "CartHeader is null.";
                    return _response;
                }
                var cartFromDb = await _db.CartHeaders.FirstAsync(u => u.UserId == cartDto.CartHeader.UserId);
                cartFromDb.CouponCode = "";
                _db.CartHeaders.Update(cartFromDb);
                await _db.SaveChangesAsync();
                _response.Data = true;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.ToString();
            }
            return _response;
        }


        [HttpPost("CartUpsert")]
        public async Task<ResponseDTO> CartUpsert(CartDTO cartDto)
        {
            try
            {
                // Prüft, ob der CartHeader im DTO vorhanden ist
                if (cartDto.CartHeader == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "CartHeader is null.";
                    return _response;
                }






                // Sucht nach einem vorhandenen CartHeader für den Benutzer in der Datenbank
                var cartHeaderFromDb = await _db.CartHeaders.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserId == cartDto.CartHeader.UserId);






                // Wenn kein CartHeader gefunden wurde, wird ein neuer erstellt
                if (cartHeaderFromDb == null)
                {
                    CartHeader cartHeader = _mapper.Map<CartHeader>(cartDto.CartHeader);
                    _db.CartHeaders.Add(cartHeader);
                    await _db.SaveChangesAsync();

                    // Prüft, ob CartDetails vorhanden sind
                    if (cartDto.CartDetails != null && cartDto.CartDetails.Any())
                    {
                        // Setzt die CartHeaderId im ersten CartDetail und speichert es
                        cartDto.CartDetails.First().CartHeaderId = cartHeader.CartHeaderId;
                        _db.CartDetails.Add(_mapper.Map<CartDetails>(cartDto.CartDetails.First()));
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        // Keine CartDetails vorhanden: Fehler zurückgeben
                        _response.IsSuccess = false;
                        _response.Message = "CartDetails is null or empty.";
                        return _response;
                    }
                }
                else
                {
                    // CartHeader existiert bereits
                    // Prüft, ob CartDetails vorhanden sind
                    if (cartDto.CartDetails != null && cartDto.CartDetails.Any())
                    {
                        var firstCartDetail = cartDto.CartDetails.First();

                        // Sucht nach einem CartDetail mit gleichem Produkt und Header
                        var cartDetailsFromDb = await _db.CartDetails.AsNoTracking().FirstOrDefaultAsync(
                            u => u.ProductId == firstCartDetail.ProductId &&
                            u.CartHeaderId == cartHeaderFromDb.CartHeaderId);

                        if (cartDetailsFromDb == null)
                        {
                            // Produkt ist noch nicht im Warenkorb: Neues CartDetail anlegen
                            firstCartDetail.CartHeaderId = cartHeaderFromDb.CartHeaderId;
                            _db.CartDetails.Add(_mapper.Map<CartDetails>(firstCartDetail));
                            await _db.SaveChangesAsync();
                        }
                        else
                        {
                            // Produkt ist schon im Warenkorb: Anzahl erhöhen und aktualisieren
                            firstCartDetail.Count += cartDetailsFromDb.Count;
                            firstCartDetail.CartHeaderId = cartDetailsFromDb.CartHeaderId;
                            firstCartDetail.CartDetailsId = cartDetailsFromDb.CartDetailsId;
                            _db.CartDetails.Update(_mapper.Map<CartDetails>(firstCartDetail));
                            await _db.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        // Keine CartDetails vorhanden: Fehler zurückgeben
                        _response.IsSuccess = false;
                        _response.Message = "CartDetails is null or empty.";
                        return _response;
                    }
                }
                _response.Data = cartDto;
            }
            catch (Exception ex)
            {
                _response.Message = ex.Message.ToString();
                _response.IsSuccess = false;
            }
            return _response;
        }


        [HttpPost("RemoveCart")]
        public async Task<ResponseDTO> RemoveCart([FromBody] int cartDetailsId)
        {
            try
            {
                CartDetails cartDetails = _db.CartDetails
                   .First(u => u.CartDetailsId == cartDetailsId);

                int totalCountofCartItem = _db.CartDetails.Where(u => u.CartHeaderId == cartDetails.CartHeaderId).Count();
                _db.CartDetails.Remove(cartDetails);
                if (totalCountofCartItem == 1)
                {
                    var cartHeaderToRemove = await _db.CartHeaders
                       .FirstOrDefaultAsync(u => u.CartHeaderId == cartDetails.CartHeaderId);

                    if (cartHeaderToRemove != null)
                    {
                        _db.CartHeaders.Remove(cartHeaderToRemove);
                    }
                }
                await _db.SaveChangesAsync();

                _response.Data = true;
            }
            catch (Exception ex)
            {
                _response.Message = ex.Message.ToString();
                _response.IsSuccess = false;
            }
            return _response;
        }


        [HttpPost("EmailCartRequest")]
        public async Task<object> EmailCartRequest([FromBody] CartDTO cartDto)
        {
            try
            {
                //await _messageBus.PublishMessage(cartDto, _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCart"));
                await _messageBus.PublishMessage(cartDto, _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue"));
                _response.Data = true;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.ToString();
            }
            return _response;
        }

    }
}
