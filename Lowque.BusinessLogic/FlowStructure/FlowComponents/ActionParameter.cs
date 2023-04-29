using System.Collections.Generic;

namespace Lowque.BusinessLogic.FlowStructure.FlowComponents
{
    public class ActionParameter
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public bool AreValuesPredefined { get; set; }

        public IEnumerable<string> PredefinedValues { get; set; }

        public bool DisableHints { get; set; }

        public bool DisableTranslating { get; set; }

        public bool HasChanged { get; set; }

        public bool IsRequired { get; set; }

        public bool IsNullable { get; set; }

        public string Type { get; set; }

        public string GenericType { get; set; }

        public string DisplayName { get; set; }

        public string DisplayDescription { get; set; }
    }
}
