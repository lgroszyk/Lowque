using Lowque.BusinessLogic.Dto.ApplicationDefinition;

namespace Lowque.BusinessLogic.Services.Interfaces
{
    public interface IApplicationDefinitionService
    {
        GetExistingApplicationsAndTemplatesResponseDto GetExistingApplicationsAndTemplates();
        GetApplicationDefinitionResponseDto GetApplicationDefinition(int id);
        GetFlowDefinitionsResponseDto GetFlowDefinitions(int id);
        CreateApplicationDefinitionResponseDto CreateApplicationDefinition(CreateApplicationDefinitionRequestDto dto);
        CreateFlowDefinitionResponseDto CreateFlowDefinition(CreateFlowDefinitionRequestDto dto);
        DeleteFlowDefinitionResponseDto DeleteFlowDefinition(int id);
        DeployApplicationResponseDto DeployApplication(DeployApplicationRequestDto dto);
        DownloadSpecificationResponseDto DownloadSpecification(int id);
    }
}
