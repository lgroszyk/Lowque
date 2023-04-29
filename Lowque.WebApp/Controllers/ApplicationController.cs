using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Lowque.BusinessLogic.Dto.Application;
using Lowque.BusinessLogic.Services.Interfaces;

namespace Lowque.WebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class ApplicationController : ControllerBase
    {
        private readonly IApplicationService applicationService;

        public ApplicationController(IApplicationService applicationService)
        {
            this.applicationService = applicationService;
        }

        [HttpGet("GetLocalizedPhrases")]
        [AllowAnonymous]
        public GetLocalizedPhrasesResponseDto GetLocalizedPhrases()
        {
            return applicationService.GetLocalizedPhrases();
        }

        [HttpGet("GetConfiguration")]
        public GetConfigurationResponseDto GetConfiguration()
        {
            return applicationService.GetConfiguration();
        }
    }
}
