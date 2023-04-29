using Microsoft.AspNetCore.Mvc;
using Lowque.BusinessLogic.Services.Interfaces;
using Lowque.BusinessLogic.Dto.Identity;

namespace Lowque.WebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IdentityController
    {
        private readonly IIdentityService identityService;

        public IdentityController(IIdentityService identityService)
        {
            this.identityService = identityService;
        }

        [HttpPost("Login")]
        public LoginResponseDto Login(LoginRequestDto dto)
        {
            return identityService.Login(dto);
        }

        [HttpPost("ChangePassword")]
        public ChangePasswordResponseDto ChangePassword(ChangePasswordRequestDto dto)
        {
            return identityService.ChangePassword(dto);
        }
    }
}
