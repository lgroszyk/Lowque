using Lowque.BusinessLogic.FlowStructure.FlowComponents;

namespace Lowque.BusinessLogic.Types
{
    public interface ISystemTypesContext
    {
        TypeData TryGetSystemTypeByName(string typeName);
    }
}
