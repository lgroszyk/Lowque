using Lowque.BusinessLogic.Dto.ApplicationDefinition;
using Lowque.BusinessLogic.FlowStructure.FlowComponents;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

/*

AddEmployeeRequestDto:
{
    FirstName: string (Required, Maximum length: 50)
	LastName: string (Required, Maximum length: 50)
	Age: int (Required)
    Departments: List<string> (Not required)
}

AddEmployeeResponseDto:
{
    NewEmployeeId: int (Required)
    Employees: List<AfterAddEmployeeInfo> (Required)
}

AfterAddEmployeeInfo
{
    EmployeeId: int (Required)
    FullName: string (Required, Maximum length: 101)
}

*/

namespace Lowque.BusinessLogic.Types
{
    public class TypeSpecificationFormatter : ITypeSpecificationFormatter
    {
        public FlowSpecificationType Format(TypeData typeData)
        {
            var result = new FlowSpecificationType
            {
                TypeName = typeData.Name,
                TypeProperties = new List<string>()
            };

            foreach (var prop in typeData.Properties)
            {
                var name = prop.Name;
                var propType = SystemBasicTypes.AsCSharpType(prop.Type);
                var type = prop.List ? $"List<{propType}>" : propType;
                var nullable = prop.Nullable ? "?" : "";
                var required = prop.Required ? "Required" : "";
                var maxLength = prop.MaxLength == null ? "" : $"Maximum length: {prop.MaxLength}";

                var detailsElements = new List<string>();
                if (!string.IsNullOrEmpty(required))
                {
                    detailsElements.Add(required);
                }
                if (!string.IsNullOrEmpty(maxLength))
                {
                    detailsElements.Add(maxLength);
                }
                var details = detailsElements.Any() ? $" ({string.Join(", ", detailsElements)})" : "";

                result.TypeProperties.Add($"{name}: {type}{nullable}{details}");
            }
            return result;
        }
    }
}
