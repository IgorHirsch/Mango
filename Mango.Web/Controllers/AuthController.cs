using Mango.Web.Models;
using Mango.Web.Models.RegisterLogin;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Mango.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ITokenProvider _tokenProvider;


        public AuthController(IAuthService authService, ITokenProvider tokenProvider)
        {
            _authService = authService;
            _tokenProvider = tokenProvider;
        }

        [HttpGet]
        public IActionResult Login()
        {
            LoginRequestDTO loginRequestDto = new();
            return View(loginRequestDto);
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestDTO obj)
        {
            ResponseDTO responseDto = await _authService.LoginAsync(obj);

            if (responseDto != null && responseDto.IsSuccess)
            {
                LoginResponseDTO loginResponseDto =
                    JsonConvert.DeserializeObject<LoginResponseDTO>(Convert.ToString(responseDto.Data));


                await SignInUser(loginResponseDto);
                _tokenProvider.SetToken(loginResponseDto.Token);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["error"] = responseDto.Message;
                return View(obj);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            var roleList = new List<SelectListItem>()
            {
                new SelectListItem{Text=SD.RoleAdmin,Value=SD.RoleAdmin},
                new SelectListItem{Text=SD.RoleCustomer,Value=SD.RoleCustomer},
            };

            ViewBag.RoleList = roleList;

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(RegistrationRequestDTO obj)
        {
            ResponseDTO result = await _authService.RegisterAsync(obj);
            ResponseDTO assingRole;

            if (result != null && result.IsSuccess)
            {
                if (string.IsNullOrEmpty(obj.Role))
                {
                    obj.Role = SD.RoleCustomer;
                }
                assingRole = await _authService.AssignRoleAsync(obj);
                if (assingRole != null && assingRole.IsSuccess)
                {
                    TempData["success"] = "Registration Successful";
                    return RedirectToAction(nameof(Login));
                }
            }

            var roleList = new List<SelectListItem>()
            {
                new SelectListItem{Text=SD.RoleAdmin,Value=SD.RoleAdmin},
                new SelectListItem{Text=SD.RoleCustomer,Value=SD.RoleCustomer},
            };

            ViewBag.RoleList = roleList;
            return View(obj);
        }



        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            _tokenProvider.ClearToken();
            return RedirectToAction("Index", "Home");
        }





        private async Task SignInUser(LoginResponseDTO model)
        {
            if (string.IsNullOrWhiteSpace(model?.Token))
                throw new ArgumentNullException(nameof(model.Token), "JWT token is null or empty.");

            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken token;

            try
            {
                token = handler.ReadJwtToken(model.Token);
            }
            catch (Exception ex)
            {
                throw new Exception("Token konnte nicht gelesen werden.", ex);
            }

            var claims = token?.Claims?.ToList();
            if (claims == null || !claims.Any())
                throw new Exception("Token enthält keine gültigen Claims.");

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);

            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Email,
                claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value ?? ""));
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Sub,
                claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value ?? ""));
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Name,
                claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name)?.Value ?? ""));

            identity.AddClaim(new Claim(ClaimTypes.Name,
                claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value ?? ""));
            identity.AddClaim(new Claim(ClaimTypes.Role,
                claims.FirstOrDefault(c => c.Type == "role")?.Value ?? ""));

            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }


    }
}
