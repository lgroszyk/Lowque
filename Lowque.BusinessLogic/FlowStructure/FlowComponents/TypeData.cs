using System.Collections.Generic;

namespace Lowque.BusinessLogic.FlowStructure.FlowComponents
{
    public class TypeData
    {
        public string Name { get; set; }

        public bool PreventDelete { get; set; }

        public bool PreventEdit { get; set; }

        public IEnumerable<TypeProperty> Properties { get; set; }
    }
}
