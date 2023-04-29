using System.ComponentModel.DataAnnotations;

namespace Lowque.BusinessLogic.Dto.ApplicationDefinition
{
    public class CreateFlowDefinitionRequestDto
    {
        [Required, StringLength(50, MinimumLength = 3), RegularExpression(DtoValidation.PascalCaseRegex)]
        public string Name { get; set; }

        [Required, StringLength(50, MinimumLength = 3), RegularExpression(DtoValidation.PascalCaseRegex)]
        public string Area { get; set; }

        [Required]
        public int ApplicationId { get; set; }
    }
}
