using Lowque.BusinessLogic.FlowStructure.FlowComponents;
using Lowque.BusinessLogic.SolutionGeneration;
using Lowque.BusinessLogic.Types;
using Lowque.DataAccess;
using Lowque.DataAccess.Internationalization.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Lowque.BusinessLogic.FlowActions.Implementation
{
    public class ChangeVariableAction : BaseAction
    {
        public ChangeVariableAction(AppDbContext dbContext, ILocalizationContext localizationContext, ISystemTypesContext systemTypesContext) : base(dbContext, localizationContext, systemTypesContext) { }
        
        private const string varNameParamName = "ChangeVariable_VariableName";
        private const string varValueSimpleParamName = "ChangeVariable_VariableValue_Simple";
        private const string varValueListParamName = "ChangeVariable_VariableValue_List";
        private const string varValueComplexParamPrefix = "ChangeVariable_VariableValue_Complex_";

        private List<ActionParameter> newParams = new List<ActionParameter>();

        public override PrecompilationValidationResult Validate()
        {
            if (!HasVariableNameParameterValidValue(varNameParamName))
            {
                return GetVariableNameInvalidError(varNameParamName);
            }

            var varName = GetParameter(varNameParamName).Value;
            var existingVarsWithGivenName = GetEditableVariables().Where(exVar => exVar.Name == varName);
            if (existingVarsWithGivenName.Count() == 0)
            {
                return PrecompilationValidationResult.Invalid($"{Name}: {localizationContext.TryTranslate("FlowDesigner_PrecompilationError_VariableDoesNotExist", varName)}");
            }
            if (existingVarsWithGivenName.Count() >= 2)
            {
                return PrecompilationValidationResult.Invalid($"{Name}: {localizationContext.TryTranslate("FlowDesigner_PrecompilationError_VariableDeclaredMoreThanOnce", varName)}");
            }

            if (TryGetParameter(varValueSimpleParamName) != null && !HasParameterValue(varValueSimpleParamName))
            {
                return GetRequiredError(varValueSimpleParamName);
            }

            if (TryGetParameter(varValueListParamName) != null && !HasParameterValue(varValueListParamName))
            {
                return GetRequiredError(varValueListParamName);
            }

            return PrecompilationValidationResult.Valid();
        }

        protected override void GenerateActionSpecificCode()
        {
            var varName = GetParameter(varNameParamName).Value;
            var chosenVar = GetEditableVariables().FirstOrDefault(exVar => exVar.Name == varName);

            if (SystemBasicTypes.IsSimpleType(chosenVar.Type))
            {
                var varValue = GetParameter(varValueSimpleParamName).Value;
                codeLines.Add(CreateCodeLine($"{varName} = {varValue};", CodeIndent.DoNotChange));
            }
            else if (SystemBasicTypes.IsList(chosenVar.Type))
            {
                var varValue = GetParameter(varValueListParamName).Value;
                codeLines.Add(CreateCodeLine($"{varName} = {varValue};", CodeIndent.DoNotChange));
            }
            else
            {
                var varPropParamsNotEmpty = GetParametersStartingWith(varValueComplexParamPrefix).Where(variable => !string.IsNullOrEmpty(variable.Value));
                foreach (var varPropParam in varPropParamsNotEmpty)
                {
                    var propName = varPropParam.Name.Split(varValueComplexParamPrefix).ElementAt(1);
                    var propValue = varPropParam.Value;
                    codeLines.Add(CreateCodeLine($"{varName}.{propName} = {propValue};", CodeIndent.DoNotChange));
                }
            }
        }

        public override void GenerateParameters()
        {
            RemoveOtherActionsParameters();

            var varNameParam = AddParameterIfNotExist(varNameParamName, GetEditableVariablesNames(), type: SystemBasicTypes.TextPhrase);
            newParams.Add(varNameParam);

            if (HasParameterChanged(varNameParamName))
            {
                RemoveAllParametersButNot(varNameParamName);
            }

            var chosenVar = GetEditableVariables().FirstOrDefault(variable => variable.Name == varNameParam.Value);
            if (chosenVar != null)
            {
                var chosenVarType = chosenVar.Type;
                if (SystemBasicTypes.IsSimpleType(chosenVarType))
                {
                    var varValueSimpleParam = AddParameterIfNotExist(varValueSimpleParamName, type: chosenVarType);
                    newParams.Add(varValueSimpleParam);
                }
                else if (SystemBasicTypes.IsList(chosenVarType))
                {
                    var varValueListParam = AddParameterIfNotExist(varValueListParamName, type: chosenVarType, genericType: chosenVar.GenericType);
                    newParams.Add(varValueListParam);
                }
                else
                {
                    var typeProps = GetComplexTypeEditableProperties(chosenVarType);
                    foreach (var typeProp in typeProps)
                    {
                        var valueComplexPropParam = AddParameterIfNotExist($"{varValueComplexParamPrefix}{typeProp.Name}",
                            disableTranslating: true, isRequired: false, type: typeProp.ActionType, genericType: typeProp.ActionGenericType);
                        newParams.Add(valueComplexPropParam);
                    }
                }
            }

            newParams.ForEach(param => param.HasChanged = false);
            Parameters = newParams;
        }

        private IEnumerable<ActionResult> GetEditableVariables()
        {
            var availableVars = new List<ActionResult>();

            IteratePreviousActions(action =>
            {
                var results = action.GetResults();
                foreach (var result in results)
                {
                    if (result.IsVariable)
                    {
                        availableVars.Add(result);
                    }
                }
                return true;
            });

            return availableVars;
        }

        private IEnumerable<string> GetEditableVariablesNames()
        {
            return GetEditableVariables().Select(exVar => exVar.Name);
        }
    }
}
