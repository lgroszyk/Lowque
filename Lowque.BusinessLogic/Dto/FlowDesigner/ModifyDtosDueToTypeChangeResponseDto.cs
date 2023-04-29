using Lowque.BusinessLogic.FlowStructure.FlowComponents;
using System.Collections.Generic;

namespace Lowque.BusinessLogic.Dto.FlowDesigner
{
    public class ModifyDtosDueToTypeChangeResponseDto
    {
        public IEnumerable<TypeData> TypesData { get; set; }
    }
}
