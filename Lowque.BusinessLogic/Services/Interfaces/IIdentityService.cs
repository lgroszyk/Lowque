using Lowque.BusinessLogic.Dto.Identity;

namespace Lowque.BusinessLogic.Services.Interfaces
{
    public interface IIdentityService
    {
        LoginResponseDto Login(LoginRequestDto dto);
        ChangePasswordResponseDto ChangePassword(ChangePasswordRequestDto dto);
    }
}
