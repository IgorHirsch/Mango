using AutoMapper;
using Mango.Services.OrderAPI.Data;
using Mango.Services.OrderAPI.Models.DTO;
using Mango.Services.OrderAPI.Models.DTO.Cart;
using Mango.Services.OrderAPI.Models.DTO.Order;
using Mango.Services.OrderAPI.Models.Order;
using Mango.Services.OrderAPI.Services.IServices;
using Mango.Services.OrderAPI.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.OrderAPI.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class OrderAPIController : ControllerBase
    {
        protected ResponseDTO _response;
        private IMapper _mapper;
        private readonly AppDbContext _db;
        private IProductService _productService;
        public OrderAPIController(AppDbContext db,
            IProductService productService, IMapper mapper)
        {
            _db = db;
            this._response = new ResponseDTO();
            _productService = productService;
            _mapper = mapper;
        }

        [Authorize]
        [HttpPost("CreateOrder")]
        public async Task<ResponseDTO> CreateOrder([FromBody] CartDTO cartDto)
        {
            try
            {
                OrderHeaderDTO orderHeaderDto = _mapper.Map<OrderHeaderDTO>(cartDto.CartHeader);
                orderHeaderDto.OrderTime = DateTime.Now;
                orderHeaderDto.Status = SD.Status_Pending;
                orderHeaderDto.OrderDetails = _mapper.Map<IEnumerable<OrderDetailsDTO>>(cartDto.CartDetails);

                OrderHeader orderCreated = _db.OrderHeaders.Add(_mapper.Map<OrderHeader>(orderHeaderDto)).Entity;
                await _db.SaveChangesAsync();

                orderHeaderDto.OrderHeaderId = orderCreated.OrderHeaderId;
                _response.Data = orderHeaderDto;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
    }
}
