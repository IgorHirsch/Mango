using Mango.Services.OrderAPI.Models.DTO;

namespace Mango.Services.OrderAPI.Services.IServices
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDTO>> GetProducts();
    }
}
