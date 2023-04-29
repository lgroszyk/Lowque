using Lowque.BusinessLogic.FlowStructure;

namespace Lowque.BusinessLogic.Dto.FlowDesigner
{
    public class GetFlowResponseDto
    {
        public FlowContent FlowData { get; set; }
        public FlowProperties FlowProperties { get; set; }
    }
}
