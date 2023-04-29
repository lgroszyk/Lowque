using Lowque.BusinessLogic.FlowStructure.FlowComponents;
using System.Collections.Generic;

namespace Lowque.BusinessLogic.FlowStructure
{
    public class FlowContent
    {
        public IEnumerable<NodeData> NodesData { get; set; }

        public IEnumerable<EdgeData> EdgesData { get; set; }

        public IEnumerable<TypeData> TypesData { get; set; }
    }
}
