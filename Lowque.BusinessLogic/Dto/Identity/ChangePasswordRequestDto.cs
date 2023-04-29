using System.ComponentModel.DataAnnotations;

namespace Lowque.BusinessLogic.Dto.Identity
{
    public class ChangePasswordRequestDto
    {
        [Required, MaxLength(50)]
        public string CurrentPassword { get; set; }

        [Required, MaxLength(50), RegularExpression(DtoValidation.PasswordRegex)]
        public string NewPassword { get; set; }

        [Required, MaxLength(50), RegularExpression(DtoValidation.PasswordRegex)]
        public string NewPasswordConfirmation { get; set; }
    }
}
