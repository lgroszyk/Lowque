using Lowque.BusinessLogic.Dto.Application;
using Lowque.BusinessLogic.Services.Interfaces;
using Lowque.DataAccess.Internationalization.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Lowque.BusinessLogic.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly ILocalizationContext localizationContext;
        private readonly IConfiguration appConfiguration;

        public ApplicationService(ILocalizationContext localizationContext, IConfiguration appConfiguration)
        {
            this.localizationContext = localizationContext;
            this.appConfiguration = appConfiguration;
        }

        public GetConfigurationResponseDto GetConfiguration()
        {
            return new GetConfigurationResponseDto
            {
                Workspace = appConfiguration["Configuration:Workspace"],
                DotnetCli = appConfiguration["Configuration:DotnetCli"]
            };
        }

        public GetLocalizedPhrasesResponseDto GetLocalizedPhrases()
        {
            var localizedPhrases = localizationContext.GetLocalizedPhrases();
            return new GetLocalizedPhrasesResponseDto
            {
                LocalizedPhrases = localizedPhrases
            };
        }
    }
}
