using Lowque.BusinessLogic.Types;

namespace Lowque.BusinessLogic.FlowStructure.FlowComponents
{
    public class TypeProperty
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public bool Required { get; set; }

        public bool Nullable { get; set; }

        public int? MaxLength { get; set; }

        public bool List { get; set; }

        public bool Navigation { get; set; }

        public bool Key { get; set; }

        public string ActionType => List ? SystemBasicTypes.List : Type;

        public string ActionGenericType => List ? Type : null;
    }
}
