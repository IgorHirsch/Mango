using Mango.Services.OrderAPI.Models.DTO.Order;

namespace Mango.Services.OrderAPI.Models.DTO.StripeDTO
{
    public class StripeRequestDTO
    {
        public string? StripeSessionUrl { get; set; }
        public string? StripeSessionId { get; set; }
        public string ApprovedUrl { get; set; }
        public string CancelUrl { get; set; }
        public OrderHeaderDTO OrderHeader { get; set; }
    }
}
