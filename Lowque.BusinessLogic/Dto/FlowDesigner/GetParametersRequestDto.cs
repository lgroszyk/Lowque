using Lowque.BusinessLogic.FlowStructure;
using System.ComponentModel.DataAnnotations;

namespace Lowque.BusinessLogic.Dto.FlowDesigner
{
    public class GetParametersRequestDto
    {
        [Required]
        public FlowProperties FlowProperties { get; set; }

        [Required]
        public FlowContent FlowData { get; set; }

        [Required]
        public string ActionId { get; set; }

        [Required]
        public string ActionType { get; set; }
    }
}
