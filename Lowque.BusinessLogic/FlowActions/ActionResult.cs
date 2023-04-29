using Lowque.BusinessLogic.Dto.FlowDesigner;
using Lowque.BusinessLogic.Types;
using Lowque.DataAccess.Internationalization.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Lowque.BusinessLogic.FlowActions
{
    public class ActionResult
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string GenericType { get; set; }
        public IEnumerable<ActionResultProperty> Properties { get; set; }
        public bool IsVariable { get; set; }

        public FormulaHintDto ToFormulaHint(ILocalizationContext localizationContext)
        {
            return new FormulaHintDto
            {
                Name = Name,
                Type = FormatType(localizationContext, Type, GenericType),
                Properties = Properties?.Select(prop => FormatPropertyName(localizationContext, prop.Name, prop.Type, prop.GenericType))
            };
        }

        private string FormatType(ILocalizationContext localizationContext, string type, string genericType)
        {
            if (SystemBasicTypes.IsSimpleType(type))
            {
                return $" - {type}";
            }
            else if (SystemBasicTypes.IsList(type))
            {
                return $" - {localizationContext.TryTranslate("FlowDesigner_SimpleType_List")} ({genericType})";
            }
            else
            {
                return $" - {localizationContext.TryTranslate("FlowDesigner_ComplexType_Struct")} ({type})";
            }
        }

        private string FormatPropertyName(ILocalizationContext localizationContext, 
            string name, string type, string genericType)
        {
            return $"{name}{FormatType(localizationContext, type, genericType)}";
        }
    }
}
