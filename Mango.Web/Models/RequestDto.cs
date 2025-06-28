using static Mango.Web.Utility.SD;

namespace Mango.Web.Models.Coupon
{
    public class RequestDto
    {
        public string? AccessToken { get; set; }
        public object? Data { get; set; }
        public ApiType ApiType { get; set; } = ApiType.GET;
        public string? Url { get; set; }
    }
}