using System.ComponentModel.DataAnnotations;

namespace Lowque.BusinessLogic.Dto.ApplicationDefinition
{
    public class CreateApplicationDefinitionRequestDto
    {
        [Required, StringLength(50, MinimumLength = 3)]
        public string Template { get; set; }
    }
}
