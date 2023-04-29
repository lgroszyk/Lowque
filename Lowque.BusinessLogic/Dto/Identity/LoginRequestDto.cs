using System.ComponentModel.DataAnnotations;

namespace Lowque.BusinessLogic.Dto.Identity
{
    public class LoginRequestDto
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
