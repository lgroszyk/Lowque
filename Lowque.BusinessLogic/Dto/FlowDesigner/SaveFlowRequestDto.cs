using Lowque.BusinessLogic.FlowStructure;
using System.ComponentModel.DataAnnotations;

namespace Lowque.BusinessLogic.Dto.FlowDesigner
{
    public class SaveFlowRequestDto
    {
        [Required]
        public FlowContent FlowData { get; set; }

        [Required]
        public FlowProperties FlowProperties { get; set; }
    }
}
