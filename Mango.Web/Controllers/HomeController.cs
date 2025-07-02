using Mango.Web.Models;
using Mango.Web.Models.Cart;
using Mango.Web.Models.Product;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Mango.Web.Controllers
{
    public class HomeController : Controller
    {


        private readonly IProductService _productService;
        private readonly ICartService _cartService;



        public HomeController(IProductService productService, ICartService cartService)
        {
            _productService = productService;
            _cartService = cartService;
        }





        public async Task<IActionResult> Index()
        {
            List<ProductDTO>? list = new();

            ResponseDTO? response = await _productService.GetAllProductsAsync();

            if (response != null && response.IsSuccess)
            {
                list = JsonConvert.DeserializeObject<List<ProductDTO>>(Convert.ToString(response.Data) ?? string.Empty);
            }
            else
            {
                TempData["error"] = response?.Message;
            }

            return View(list);
        }





        [Authorize]
        public async Task<IActionResult> ProductDetails(int productId)
        {
            ProductDTO? model = new();

            ResponseDTO? response = await _productService.GetProductByIdAsync(productId);

            if (response != null && response.IsSuccess)
            {
                model = JsonConvert.DeserializeObject<ProductDTO>(Convert.ToString(response.Data)?? string.Empty);
            }
            else
            {
                TempData["error"] = response?.Message;
            }

            return View(model);
        }
        [Authorize]

        [HttpPost]
        [ActionName("ProductDetails")]
        public async Task<IActionResult> ProductDetails(ProductDTO productDto)
        {
            // Erstellt ein neues CartDTO-Objekt mit dem aktuellen Benutzer als CartHeader
            // Liest die UserId aus dem Authentifizierungs-Token ("sub"-Claim)
            CartDTO cartDto = new CartDTO()
            {
                CartHeader = new CartHeaderDTO
                {
                    UserId = User.FindFirst("sub")?.Value
                }
            };

            
            CartDetailsDTO cartDetails = new CartDetailsDTO()
            {
                Count = productDto.Count,
                ProductId = productDto.ProductId,
            };

            
            List<CartDetailsDTO> cartDetailsDtos = new() { cartDetails };
            cartDto.CartDetails = cartDetailsDtos;

            // Sendet den aktuellen Warenkorb (cartDto) an den Cart-Service, um ihn zu speichern oder zu aktualisieren
            ResponseDTO? response = await _cartService.UpsertCartAsync(cartDto);

            // Prüft das Ergebnis und zeigt eine Erfolgsmeldung oder einen Fehler an
            if (response != null && response.IsSuccess)
            {       
                TempData["success"] = "Item has been added to the Shopping Cart";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["error"] = response?.Message;
            }

            // Gibt das Produktmodell zurück, falls ein Fehler aufgetreten ist
            return View(productDto);
        }













        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
