using Lowque.BusinessLogic.SolutionGeneration;
using Lowque.DataAccess.SolutionCompilation;
using System.Collections.Generic;

namespace Lowque.BusinessLogic.SolutionCompilation
{
    public interface ICompilationErrorsFormatter
    {
        IEnumerable<CompilationError> Format(GenerationResult generationResult, CompilationResult compilationResult);
    }
}
