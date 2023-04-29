using Lowque.BusinessLogic.FlowStructure.FlowComponents;
using System.Collections.Generic;

namespace Lowque.BusinessLogic.Dto.FlowDesigner
{
    public class GetParametersResponseDto
    {
        public string ActionId { get; set; }
        public string ActionType { get; set; }
        public IEnumerable<ActionParameter> UpdatedParameters { get; set; }
        public IEnumerable<FormulaHintDto> FormulaHints { get; set; }
    }

    public class FormulaHintDto
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public IEnumerable<string> Properties { get; set; }
    }
}
