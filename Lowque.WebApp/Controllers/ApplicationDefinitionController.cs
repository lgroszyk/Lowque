using Lowque.BusinessLogic.Dto.ApplicationDefinition;
using Lowque.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lowque.WebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class ApplicationDefinitionController : ControllerBase
    {
        private readonly IApplicationDefinitionService applicationDefinitionService;

        public ApplicationDefinitionController(IApplicationDefinitionService applicationDefinitionService)
        {
            this.applicationDefinitionService = applicationDefinitionService;
        }

        [HttpPost("CreateApplicationDefinition")]
        public CreateApplicationDefinitionResponseDto CreateApplicationDefinition(CreateApplicationDefinitionRequestDto dto)
        {
            return applicationDefinitionService.CreateApplicationDefinition(dto);
        }

        [HttpGet("GetExistingApplicationsAndTemplates")]
        public GetExistingApplicationsAndTemplatesResponseDto GetExistingApplicationsAndTemplates()
        {
            return applicationDefinitionService.GetExistingApplicationsAndTemplates();
        }

        [HttpGet("GetApplicationDefinition/{id}")]
        public GetApplicationDefinitionResponseDto GetApplicationDefinition(int id)
        {
            return applicationDefinitionService.GetApplicationDefinition(id);
        }

        [HttpGet("GetFlowDefinitions/{id}")]
        public GetFlowDefinitionsResponseDto GetFlowDefinitions(int id)
        {
            return applicationDefinitionService.GetFlowDefinitions(id);
        }

        [HttpPost("CreateFlowDefinition")]
        public CreateFlowDefinitionResponseDto CreateFlowDefinition(CreateFlowDefinitionRequestDto dto)
        {
            return applicationDefinitionService.CreateFlowDefinition(dto);
        }

        [HttpDelete("DeleteFlowDefinition/{id}")]
        public DeleteFlowDefinitionResponseDto DeleteFlowDefinition(int id)
        {
            return applicationDefinitionService.DeleteFlowDefinition(id);
        }

        [HttpPost("DeployApplication")]
        public DeployApplicationResponseDto DeployApplication(DeployApplicationRequestDto dto)
        {
            return applicationDefinitionService.DeployApplication(dto);
        }

        [HttpGet("DownloadSpecification/{id}")]
        public PhysicalFileResult DownloadSpecification(int id)
        {
            var responseData = applicationDefinitionService.DownloadSpecification(id);
            return PhysicalFile(responseData.FilePath, responseData.FileType, responseData.FileName);
        }

    }
}
