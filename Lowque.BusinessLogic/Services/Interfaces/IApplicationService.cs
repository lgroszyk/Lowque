using Lowque.BusinessLogic.Dto.Application;

namespace Lowque.BusinessLogic.Services.Interfaces
{
    public interface IApplicationService
    {
        GetLocalizedPhrasesResponseDto GetLocalizedPhrases();
        GetConfigurationResponseDto GetConfiguration();
    }
}
