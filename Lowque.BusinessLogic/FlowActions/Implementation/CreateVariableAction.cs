using Lowque.BusinessLogic.FlowStructure.FlowComponents;
using Lowque.BusinessLogic.SolutionGeneration;
using Lowque.BusinessLogic.Types;
using Lowque.DataAccess;
using Lowque.DataAccess.Internationalization.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Lowque.BusinessLogic.FlowActions.Implementation
{
    public class CreateVariableAction : BaseAction
    {
        public CreateVariableAction(AppDbContext dbContext, ILocalizationContext localizationContext, ISystemTypesContext systemTypesContext) : base(dbContext, localizationContext, systemTypesContext) { }

        private const string varNameParamName = "CreateVariable_VariableName";
        private const string varTypeParamName = "CreateVariable_VariableType";
        private const string varListTypeParamName = "CreateVariable_VariableType_ListType";
        private const string varSimpleValueParamName = "CreateVariable_VariableValue_Simple";
        private const string varComplexValuePropParamNamePrefix = "CreateVariable_VariableValue_Complex_";
        private const string varListValueParamName = "CreateVariable_VariableValue_List";

        private List<ActionParameter> newParams = new List<ActionParameter>();

        public override PrecompilationValidationResult Validate()
        {
            if (!HasVariableNameParameterValidValue(varNameParamName))
            {
                return GetVariableNameInvalidError(varNameParamName);
            }

            if (!HasParameterValidValue(varTypeParamName, GetAvailableVariableTypes()))
            {
                return GetInvalidValueError(varTypeParamName);
            }

            var varType = GetParameter(varTypeParamName).Value;
            if (SystemBasicTypes.IsList(varType) && 
                !HasParameterValidValue(varListTypeParamName, GetAvailableGenericTypes()))
            {
                return GetRequiredError(varListTypeParamName);
            }

            return PrecompilationValidationResult.Valid();
        }

        public override IEnumerable<ActionResult> GetResults()
        {
            if (!HasParameterValue(varNameParamName) || !HasParameterValue(varTypeParamName))
            {
                return Enumerable.Empty<ActionResult>();
            }

            var varNameParam = GetParameter(varNameParamName);
            var varTypeParam = GetParameter(varTypeParamName);

            ActionResult result;
            if (SystemBasicTypes.IsList(varTypeParam.Value))
            {
                if (!HasParameterValue(varListTypeParamName))
                {
                    return Enumerable.Empty<ActionResult>();
                }

                var listTypeParam = GetParameter(varListTypeParamName);
                result = new ActionResult
                {
                    Name = varNameParam.Value,
                    Type = SystemBasicTypes.List,
                    GenericType = listTypeParam.Value,
                    IsVariable = true,
                    Properties = SystemBasicTypes.IsSimpleType(listTypeParam.Value) 
                        ? null : GetComplexTypeActionResultProperties(listTypeParam.Value)
                };
            }
            else
            {
                result = new ActionResult
                {
                    Name = varNameParam.Value,
                    Type = varTypeParam.Value,
                    IsVariable = true,
                    Properties = SystemBasicTypes.IsSimpleType(varTypeParam.Value)
                        ? null : GetComplexTypeActionResultProperties(varTypeParam.Value)
                };
            }

            return new[] { result };
        }

        protected override void GenerateActionSpecificCode()
        {
            var varNameParam = GetParameter(varNameParamName);
            var varTypeParam = GetParameter(varTypeParamName);

            if (SystemBasicTypes.IsSimpleType(varTypeParam.Value))
            {
                var varSimpleValueParam = GetParameter(varSimpleValueParamName);

                var varType = SystemBasicTypes.AsCSharpType(varTypeParam.Value);
                var varName = varNameParam.Value;
                var varValue = varSimpleValueParam.Value;

                var varInitLine = HasParameterValue(varSimpleValueParamName)
                    ? $"{varType} {varName} = {varValue};"
                    : $"{varType} {varName};";
                codeLines.Add(CreateCodeLine(varInitLine, CodeIndent.DoNotChange));
            }
            else if (SystemBasicTypes.IsList(varTypeParam.Value))
            {
                var varListTypeParam = GetParameter(varListTypeParamName);

                var varName = varNameParam.Value;
                var varListType = SystemBasicTypes.IsSimpleType(varListTypeParam.Value) 
                    ? SystemBasicTypes.AsCSharpType(varListTypeParam.Value) 
                    : varListTypeParam.Value;
                var varListValue = GetParameter(varListValueParamName).Value;

                var varInitLine = string.IsNullOrEmpty(varListValue)
                    ? $"var {varName} = new List<{varListType}>();"
                    : $"var {varName} = {varListValue};";
                codeLines.Add(CreateCodeLine(varInitLine, CodeIndent.DoNotChange));
            }
            else
            {
                var notEmptyPropsParams = Parameters.Where(param => 
                    param.Name.StartsWith(varComplexValuePropParamNamePrefix) && param.Value != null);

                codeLines.Add(CreateCodeLine($"var {varNameParam.Value} = new {varTypeParam.Value}", CodeIndent.DoNotChange));
                codeLines.Add(CreateCodeLine("{", CodeIndent.AddAfter));
                foreach (var prop in notEmptyPropsParams)
                {
                    var varPropName = prop.Name.Split(varComplexValuePropParamNamePrefix).ElementAt(1);
                    var varPropValue = prop.Value;
                    codeLines.Add(CreateCodeLine($"{varPropName} = {varPropValue},", CodeIndent.DoNotChange));
                }
                codeLines.Add(CreateCodeLine("};", CodeIndent.DeleteBefore));
            }
        }

        public override void GenerateParameters()
        {
            RemoveOtherActionsParameters();

            var varNameParam = AddParameterIfNotExist(varNameParamName, disableHints: true, type: SystemBasicTypes.TextPhrase);
            newParams.Add(varNameParam);

            var varTypeParam = AddParameterIfNotExist(varTypeParamName, GetAvailableVariableTypes());
            newParams.Add(varTypeParam);

            if (HasParameterChanged(varTypeParamName))
            {
                RemoveAllParametersButNot(varNameParamName, varTypeParamName);
            }

            if (SystemBasicTypes.IsSimpleType(varTypeParam.Value))
            {
                var varSimpleValueParam = AddParameterIfNotExist(varSimpleValueParamName, isRequired: false, type: varTypeParam.Value);
                newParams.Add(varSimpleValueParam);
            }
            else if (SystemBasicTypes.IsList(varTypeParam.Value))
            {
                var varListTypeParam = AddParameterIfNotExist(varListTypeParamName, GetAvailableGenericTypes());
                newParams.Add(varListTypeParam);
                if (HasParameterChanged(varListTypeParamName))
                {
                    RemoveParameterIfExist(varListValueParamName);
                }

                var varListValueParam = AddParameterIfNotExist(varListValueParamName, 
                    isRequired: false, type: SystemBasicTypes.List, genericType: varListTypeParam.Value);
                newParams.Add(varListValueParam);
            }
            else
            {
                foreach (var prop in GetComplexTypeEditableProperties(varTypeParam.Value))
                {
                    var propParam = AddParameterIfNotExist($"{varComplexValuePropParamNamePrefix}{prop.Name}", 
                        disableTranslating: true, isRequired: false, type: prop.ActionType, genericType: prop.ActionGenericType, isNullable: prop.Nullable );
                    newParams.Add(propParam);
                }
            }

            newParams.ForEach(param => param.HasChanged = false);
            Parameters = newParams;
        }

        private IEnumerable<string> GetAvailableVariableTypes()
        {
            var availableBasicTypesNames = new string[] { SystemBasicTypes.Binary, SystemBasicTypes.IntegralNumber, SystemBasicTypes.RealNumber,
                SystemBasicTypes.TextPhrase, SystemBasicTypes.DateAndTime, SystemBasicTypes.File, SystemBasicTypes.List };
            var availableComplexTypesNames = GetAvailableComplexTypesNames();
            return availableBasicTypesNames.Union(availableComplexTypesNames);
        }

        private IEnumerable<string> GetAvailableGenericTypes()
        {
            return GetAvailableVariableTypes().Where(x => x != SystemBasicTypes.List);
        }
    }
}
