﻿namespace Mango.Services.OrderAPI.Models.DTO.Cart
{
    public class CartDTO
    {
        public CartHeaderDTO? CartHeader { get; set; }
        public IEnumerable<CartDetailsDTO>? CartDetails { get; set; }
    }
}
