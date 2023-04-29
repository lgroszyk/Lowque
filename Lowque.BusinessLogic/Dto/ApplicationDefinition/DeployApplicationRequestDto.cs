using System.ComponentModel.DataAnnotations;

namespace Lowque.BusinessLogic.Dto.ApplicationDefinition
{
    public class DeployApplicationRequestDto
    {
        [Required]
        public int AppId { get; set; }
    }
}
