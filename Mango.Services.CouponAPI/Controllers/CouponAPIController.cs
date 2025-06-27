using AutoMapper;
using Mango.Services.CouponAPI.Data;
using Mango.Services.CouponAPI.Models;
using Mango.Services.CouponAPI.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.CouponAPI.Controllers
{
    [Route("api/coupon")]
    [ApiController]
    [Authorize]
    public class CouponAPIController(AppDbContext db, IMapper mapper) : ControllerBase
    {
        private readonly AppDbContext _db = db;
        private readonly IMapper _mapper = mapper;

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
        [Authorize(Roles = "ADMIN")]
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
        [Authorize(Roles = "ADMIN")]
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
                coupon.CouponCode = couponDto.CouponCode ?? string.Empty;
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
