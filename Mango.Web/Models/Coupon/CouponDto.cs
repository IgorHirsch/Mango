namespace Mango.Web.Models.Coupon
{
    public class CouponDto
    {
        public int CouponId { get; set; }
        public string CouponCode { get; set; } = string.Empty; // Default-Wert hinzugefügt
        public double DiscountAmount { get; set; }
        public int MinAmount { get; set; }
    }
}