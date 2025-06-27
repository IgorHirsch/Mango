using System.ComponentModel.DataAnnotations;

namespace Mango.Web.Models.RegisterLogin
{
    public class LoginRequestDTO
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
