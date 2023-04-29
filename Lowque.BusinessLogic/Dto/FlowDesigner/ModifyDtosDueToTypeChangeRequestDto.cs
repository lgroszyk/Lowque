using Lowque.BusinessLogic.FlowStructure;
using Lowque.BusinessLogic.FlowStructure.FlowComponents;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lowque.BusinessLogic.Dto.FlowDesigner
{
    public class ModifyDtosDueToTypeChangeRequestDto
    {
        [Required]
        public string NewType { get; set; }

        [Required]
        public FlowProperties FlowProperties { get; set; }

        [Required]
        public IEnumerable<TypeData> TypesData { get; set; }
    }
}
