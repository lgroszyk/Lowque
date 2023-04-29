using System.Collections.Generic;

namespace Lowque.BusinessLogic.FlowStructure.FlowComponents
{
    public class NodeDataData
    {
        public string Label { get; set; }

        public string Type { get; set; }

        public IEnumerable<ActionParameter> Parameters { get; set; }
    }
}
