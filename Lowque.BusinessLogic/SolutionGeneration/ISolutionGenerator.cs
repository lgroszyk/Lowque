using Lowque.DataAccess.Entities;
using Lowque.DataAccess.SolutionCompilation;

namespace Lowque.BusinessLogic.SolutionGeneration
{
    public interface ISolutionGenerator
    {
        GenerationResult Generate(ApplicationDefinition appDefinition);
        GenerationResult Generate(ApplicationDefinition appDefinition, SolutionOptions options);
    }
}
