using System.ComponentModel.DataAnnotations;

namespace Lowque.BusinessLogic.Dto.FlowDesigner
{
    public class CreateNewActionRequestDto
    {
        [Required]
        public string FlowName { get; set; }
    }
}
