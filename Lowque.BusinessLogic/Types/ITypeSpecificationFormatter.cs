using Lowque.BusinessLogic.Dto.ApplicationDefinition;
using Lowque.BusinessLogic.FlowStructure.FlowComponents;

namespace Lowque.BusinessLogic.Types
{
    public interface ITypeSpecificationFormatter
    {
        FlowSpecificationType Format(TypeData typeData);
    }
}
