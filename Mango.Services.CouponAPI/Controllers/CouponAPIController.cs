using AutoMapper;
using Mango.Services.CouponAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.CouponAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CouponAPIController : ControllerBase
    {
        private readonly AppDbContext _db;
        private IMapper _mapper;

        public CouponAPIController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }




        [HttpGet]
        public ActionResult<ApiResponse<IEnumerable<CouponDto>>> Get()
        {
            var response = new ApiResponse<IEnumerable<CouponDto>>();
            try
            {
                var coupons = _db.Coupons.ToList();
                response.Data = _mapper.Map<List<CouponDto>>(coupons);
                response.Message = "Coupons erfolgreich abgerufen";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                return StatusCode(500, response);
            }
            return Ok(response);
        }




        [HttpGet("{id:int}")]
        public ActionResult<ApiResponse<CouponDto>> Get(int id)
        {
            var response = new ApiResponse<CouponDto>();
            try
            {
                var coupon = _db.Coupons.FirstOrDefault(u => u.CouponId == id);
                if (coupon == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Coupon nicht gefunden";
                    return NotFound(response);
                }
                response.Data = _mapper.Map<CouponDto>(coupon);
                response.Message = "Coupon erfolgreich abgerufen";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                return StatusCode(500, response);
            }
            return Ok(response);
        }




        [HttpPost]
        public ActionResult<ApiResponse<CouponDto>> Post([FromBody] CouponDto couponDto)
        {
            var response = new ApiResponse<CouponDto>();
            try
            {
                couponDto.CouponId = 0;
                var coupon = _mapper.Map<Coupon>(couponDto);
                _db.Coupons.Add(coupon);
                _db.SaveChanges();
                response.Data = _mapper.Map<CouponDto>(coupon);
                response.Message = "Coupon erfolgreich erstellt";
                return CreatedAtAction(nameof(Get), new { id = coupon.CouponId }, response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                return StatusCode(500, response);
            }
        }




        [HttpPut("{id:int}")]
        public ActionResult<ApiResponse<CouponDto>> Put(int id, [FromBody] CouponDto couponDto)
        {
            var response = new ApiResponse<CouponDto>();
            try
            {
                var coupon = _db.Coupons.FirstOrDefault(u => u.CouponId == id);
                if (coupon == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Coupon nicht gefunden";
                    return NotFound(response);
                }
                coupon.CouponCode = couponDto.CouponCode;
                coupon.DiscountAmount = couponDto.DiscountAmount;
                coupon.MinAmount = couponDto.MinAmount;
                _db.SaveChanges();
                response.Data = _mapper.Map<CouponDto>(coupon);
                response.Message = "Coupon erfolgreich aktualisiert";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                return StatusCode(500, response);
            }
            return Ok(response);
        }




        [HttpDelete("{id:int}")]
        public ActionResult<ApiResponse<bool>> Delete(int id)
        {
            var response = new ApiResponse<bool>();
            try
            {
                var coupon = _db.Coupons.FirstOrDefault(u => u.CouponId == id);
                if (coupon == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Coupon nicht gefunden";
                    response.Data = false;
                    return NotFound(response);
                }
                _db.Coupons.Remove(coupon);
                _db.SaveChanges();
                response.Data = true;
                response.Message = "Coupon erfolgreich gelöscht";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                response.Data = false;
                return StatusCode(500, response);
            }
            return Ok(response);
        }
    }
}
