using Lowque.BusinessLogic.Utils;
using System.Collections.Generic;

namespace Lowque.BusinessLogic.SolutionGeneration
{
    public class ServicesImplementationResult
    {
        public bool Success { get; set; }
        public IEnumerable<FlowError> Errors { get; set; }
        public IEnumerable<CodeLine> Code { get; set; }
    }
}
