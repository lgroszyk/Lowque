using System.Collections.Generic;

namespace Lowque.BusinessLogic.SolutionGeneration
{
    public class GenerationResult
    {
        public bool Success { get; set; }
        public IEnumerable<TemplateError> TemplateGenerationErrors { get; set; }
        public IEnumerable<FlowError> FlowGenerationErrors { get; set; }
        public IEnumerable<FlowAreaFlowActionCodeMap> FlowGenerationMaps { get; set; }
    }
}
