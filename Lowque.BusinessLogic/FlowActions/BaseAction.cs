using Lowque.BusinessLogic.Dto.FlowDesigner;
using Lowque.BusinessLogic.FlowStructure;
using Lowque.BusinessLogic.FlowStructure.FlowComponents;
using Lowque.BusinessLogic.SolutionGeneration;
using Lowque.BusinessLogic.Types;
using Lowque.BusinessLogic.Utils;
using Lowque.DataAccess;
using Lowque.DataAccess.Internationalization.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lowque.BusinessLogic.FlowActions
{
    public abstract class BaseAction
    {
        protected readonly AppDbContext dbContext;
        protected readonly ILocalizationContext localizationContext;
        protected readonly ISystemTypesContext systemTypesContext;

        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public IEnumerable<ActionReference> PreviousActions { get; set; }
        public IEnumerable<ActionReference> NextActions { get; set; }
        public List<ActionParameter> Parameters { get; set; }
        public FlowContext Context { get; set; }

        protected List<CodeLine> codeLines;

        public BaseAction(AppDbContext dbContext,
            ILocalizationContext localizationContext,
            ISystemTypesContext systemTypesContext)
        {
            this.dbContext = dbContext;
            this.localizationContext = localizationContext;
            this.systemTypesContext = systemTypesContext;
        }

        public virtual IEnumerable<string> GetDependencyFields()
        {
            return Enumerable.Empty<string>();
        }

        public virtual PrecompilationValidationResult Validate()
        {
            return PrecompilationValidationResult.Valid();
        }

        public virtual IEnumerable<ActionResult> GetResults()
        {
            return Enumerable.Empty<ActionResult>();
        }

        public virtual ActionResult GetSelfResult()
        {
            return null;
        }

        private ActionResult GetFlowParameter()
        {
            ActionResult flowParameter = null;
            if (Context.FlowProperties.FlowType == FlowTypes.GetResource &&
                Context.FlowProperties.UseResourceIdentifier)
            {
                flowParameter = new ActionResult
                {
                    Name = "id",
                    Type = SystemBasicTypes.IntegralNumber
                };
            }
            else
            {
                flowParameter = new ActionResult
                {
                    Name = "dto",
                    Type = $"{Context.FlowProperties.FlowName}RequestDto",
                    Properties = GetComplexTypeProperties($"{Context.FlowProperties.FlowName}RequestDto")
                        .Select(prop => new ActionResultProperty
                        {
                            Name = prop.Name,
                            Type = prop.List ? SystemBasicTypes.List : prop.Type,
                            GenericType = prop.List ? prop.Type : null
                        })
                };
            }
            return flowParameter;
        }

        public virtual void GenerateParameters()
        {

        }

        public virtual List<CodeLine> GenerateCode()
        {
            codeLines = new List<CodeLine>();
            GenerateActionSpecificCode();
            GenerateNextActionsCode();
            return codeLines;
        }

        protected abstract void GenerateActionSpecificCode();

        protected virtual void GenerateNextActionsCode()
        {
            var nextAction = TryFindNextAction();
            if (nextAction != null)
            {
                codeLines.AddRange(nextAction.GenerateCode());
            }
        }

        protected virtual BaseAction TryFindNextAction()
        {
            var nextActionRef = NextActions.FirstOrDefault();
            if (nextActionRef == null)
            {
                return null;
            }
            var nextAction = TryGetActionFromReference(nextActionRef);
            return nextAction;
        }

        protected BaseAction TryFindNextActionOfType(string actionType)
        {
            BaseAction nextAction = null;
            IterateNextActions(action =>
            {
                if (action.Type == actionType)
                {
                    nextAction = action;
                    return false;
                }
                return true;
            });
            return nextAction;
        }

        protected BaseAction TryGetActionFromReference(ActionReference actionReference)
        {
            return Context.Actions.SingleOrDefault(x => x.Id == actionReference.Id);
        }

        protected ActionParameter AddParameterIfNotExist(string parameterName, IEnumerable<string> availableValues = null,
            bool disableHints = false, bool disableTranslating = false,
            bool isRequired = true, bool isNullable = false, string type = null, string genericType = null)
        {
            return AddParameterIfNotExist(parameterName, new ActionParameter
            {
                Name = parameterName,
                Value = availableValues?.FirstOrDefault(),
                PredefinedValues = availableValues,
                AreValuesPredefined = availableValues != null,
                DisableHints = disableHints,
                DisableTranslating = disableTranslating,
                HasChanged = false,
                IsRequired = isRequired,
                IsNullable = isNullable,
                Type = type,
                GenericType = genericType
            });
        }

        private ActionParameter AddParameterIfNotExist(string parameterName, ActionParameter actionParameter)
        {
            var parameter = Parameters.SingleOrDefault(parameter => parameter.Name == parameterName);
            var parameterExists = parameter != null;

            if (!parameterExists)
            {
                parameter = actionParameter;
            }

            if (actionParameter.AreValuesPredefined)
            {
                parameter.PredefinedValues = actionParameter.PredefinedValues;
            }

            if (!parameterExists)
            {
                Parameters.Add(parameter);
            }

            return parameter;
        }

        protected void RemoveParameterIfExist(string parameterName)
        {
            var parameter = Parameters.SingleOrDefault(parameter => parameter.Name == parameterName);
            if (parameter != null)
            {
                Parameters.Remove(parameter);
            }
        }

        protected bool HasParameterChanged(string parameterName)
        {
            var parameter = Parameters.SingleOrDefault(parameter => parameter.Name == parameterName);
            return parameter != null && parameter.HasChanged;
        }

        protected IEnumerable<string> GetAvailableComplexTypesNames()
        {
            var flowTypesNames = GetAvailableFlowTypesNames();
            var databaseTypesNames = GetAvailableDatabaseTypesNames();
            return flowTypesNames.Union(databaseTypesNames);
        }

        protected IEnumerable<string> GetAvailableDatabaseTypesNames()
        {
            var dataAccessModuleDefinition = dbContext.ApplicationDefinitions
                .Single(app => app.ApplicationDefinitionId == Context.FlowProperties.AppId)
                .DataAccessModuleDefinition;
            var databaseTypes = JsonConvert.DeserializeObject<IEnumerable<TypeData>>(dataAccessModuleDefinition);
            var availableTypesNames = databaseTypes.Select(type => type.Name);
            return availableTypesNames;
        }

        protected IEnumerable<string> GetAvailableFlowTypesNames()
        {
            return Context.Types.Select(type => type.Name);
        }

        protected IEnumerable<string> GetComplexTypePropertiesNames(string typeName)
        {
            return GetComplexTypeProperties(typeName)?.Select(prop => prop.Name);
        }

        protected IEnumerable<TypeProperty> GetComplexTypeProperties(string typeName)
        {
            var flowType = Context.Types.SingleOrDefault(type => type.Name == typeName);
            if (flowType != null)
            {
                return flowType.Properties;
            }

            var dataAccessModuleDefinition = dbContext.ApplicationDefinitions
                .Single(app => app.ApplicationDefinitionId == Context.FlowProperties.AppId)
                .DataAccessModuleDefinition;
            var dbTypes = JsonConvert.DeserializeObject<IEnumerable<TypeData>>(dataAccessModuleDefinition);
            var dbType = dbTypes.SingleOrDefault(type => type.Name == typeName);
            if (dbType != null)
            {
                return dbType.Properties;
            }

            var systemType = systemTypesContext.TryGetSystemTypeByName(typeName);
            if (systemType != null)
            {
                return systemType.Properties;
            }

            return Enumerable.Empty<TypeProperty>();
        }

        protected IEnumerable<TypeProperty> GetComplexTypeEditableProperties(string typeName)
        {
            return GetComplexTypeProperties(typeName).Where(prop => !prop.Navigation);
        }

        protected IEnumerable<ActionResultProperty> GetComplexTypeActionResultProperties(string typeName)
        {
            return GetComplexTypeProperties(typeName)
                .Select(prop => new ActionResultProperty
                {
                    Name = prop.Name,
                    Type = prop.List ? SystemBasicTypes.List : prop.Type,
                    GenericType = prop.List ? prop.Type : null
                });
        }

        public void IterateNextActions(Predicate<BaseAction> operationOnPreviousAction)
        {
            var includeInSearch = true;

            var indentLevel = 0;
            if (Type == "IfElseStart" || Type == "LoopStart")
            {
                indentLevel++;
            }

            if (Type == "IfElseEnd" || Type == "LoopEnd")
            {
                indentLevel--;
            }

            var actionRef = NextActions.FirstOrDefault();
            BaseAction action = actionRef == null ? null : TryGetActionFromReference(actionRef);

            while (true)
            {
                if (actionRef == null)
                {
                    break;
                }

                if (action.Type == "IfElseEnd" || action.Type == "LoopEnd")
                {
                    indentLevel--;
                    includeInSearch = true;
                }

                if (action.Type == "IfElseStart" || action.Type == "LoopStart")
                {
                    indentLevel++;
                    includeInSearch = false;
                }

                if (includeInSearch && indentLevel <= 0)
                {
                    var shouldContinue = operationOnPreviousAction(action);
                    if (!shouldContinue)
                    {
                        break;
                    }
                }

                actionRef = action.NextActions.FirstOrDefault();
                action = actionRef == null ? null : TryGetActionFromReference(actionRef);
            }
        }

        protected void IteratePreviousActions(Predicate<BaseAction> operationOnPreviousAction)
        {
            var includeInSearch = true;

            var indentLevel = 0;
            if (Type == "IfElseStart" || Type == "LoopStart")
            {
                indentLevel--;
            }

            if (Type == "IfElseEnd" || Type == "LoopEnd")
            {
                indentLevel++;
            }

            var actionRef = PreviousActions.FirstOrDefault();
            BaseAction action = actionRef == null ? null : TryGetActionFromReference(actionRef);

            while (true)
            {
                if (actionRef == null)
                {
                    break;
                }

                if (action.Type == "IfElseStart" || action.Type == "LoopStart")
                {
                    indentLevel--;
                    includeInSearch = true;
                }

                if (action.Type == "IfElseEnd" || action.Type == "LoopEnd")
                {
                    indentLevel++;
                    includeInSearch = false;
                }

                if (includeInSearch && indentLevel <= 0)
                {
                    var shouldContinue = operationOnPreviousAction(action);
                    if (!shouldContinue)
                    {
                        break;
                    }
                }

                actionRef = action.PreviousActions.FirstOrDefault();
                action = actionRef == null ? null : TryGetActionFromReference(actionRef);
            }
        }

        public virtual IEnumerable<FormulaHintDto> GetFormulaHints()
        {
            var hints = new List<FormulaHintDto>();

            var selfResult = GetSelfResult();
            if (selfResult != null)
            {
                hints.Add(selfResult.ToFormulaHint(localizationContext));
            }

            IteratePreviousActions(action =>
            {
                var results = action.GetResults();
                foreach (var result in results)
                {
                    hints.Add(result.ToFormulaHint(localizationContext));
                }
                return true;
            });

            var flowParameter = GetFlowParameter();
            if (flowParameter != null)
            {
                hints.Add(flowParameter.ToFormulaHint(localizationContext));
            }

            return hints;
        }

        protected void RemoveOtherActionsParameters()
        {
            Parameters = Parameters
                .Where(parameter => parameter.Name.StartsWith(Type))
                .ToList();
        }

        protected CodeLine CreateCodeLine(string content, CodeIndent indent)
        {
            var codeLine = new CodeLine(content, indent, Context.FlowProperties.FlowName, Name);
            return codeLine;
        }

        protected bool HasParameterValue(string parameterName)
        {
            var param = Parameters.SingleOrDefault(param => param.Name == parameterName);
            var hasValue = param != null && !string.IsNullOrEmpty(param.Value);
            return hasValue;
        }

        protected bool HasParameterValidValue(string parameterName, IEnumerable<string> allowedValues)
        {
            var param = Parameters.SingleOrDefault(param => param.Name == parameterName);
            var hasValue = param != null && !string.IsNullOrEmpty(param.Value);
            if (!hasValue)
            {
                return false;
            }
            var isValueValid = allowedValues.Any(allowedValue => allowedValue == param.Value);
            if (!isValueValid)
            {
                return false;
            }
            return true;
        }

        protected bool HasVariableNameParameterValidValue(string parameterName)
        {
            var param = Parameters.SingleOrDefault(param => param.Name == parameterName);
            var hasValue = param != null && !string.IsNullOrEmpty(param.Value);
            if (!hasValue)
            {
                return false;
            }
            var isCamelCase = param.Value.IsCamelCase();
            return isCamelCase;
        }

        protected PrecompilationValidationResult GetRequiredError(string parameterName)
        {
            return GetPrecompilationError("FlowDesigner_PrecompilationError_IsRequired", parameterName);
        }

        protected PrecompilationValidationResult GetNotCamelCaseError(string parameterName)
        {
            return GetPrecompilationError("FlowDesigner_PrecompilationError_IsNotCamelCase", parameterName);
        }

        protected PrecompilationValidationResult GetInvalidValueError(string parameterName)
        {
            return GetPrecompilationError("FlowDesigner_PrecompilationError_IsInvalid", parameterName);
        }

        protected PrecompilationValidationResult GetVariableNameInvalidError(string parameterName)
        {
            if (!HasParameterValue(parameterName))
            {
                return GetRequiredError(parameterName);
            }
            return GetNotCamelCaseError(parameterName);
        }

        private PrecompilationValidationResult GetPrecompilationError(string errorMessage, string parameterName)
        {
            var paramLocalized = localizationContext.TryTranslate(parameterName);
            var message = $"{Name}: {localizationContext.TryTranslate(errorMessage, paramLocalized)}";
            var error = PrecompilationValidationResult.Invalid(message);
            return error;
        }

        protected ActionParameter GetParameter(string parameterName)
        {
            return Parameters.Single(param => param.Name == parameterName);
        }

        protected ActionParameter TryGetParameter(string parameterName)
        {
            return Parameters.SingleOrDefault(param => param.Name == parameterName);
        }

        protected IEnumerable<ActionParameter> GetParametersStartingWith(string parameterNamePrefix)
        {
            return Parameters.Where(param => param.Name.StartsWith($"{parameterNamePrefix}"));
        }

        protected IEnumerable<string> GetBooleanValues()
        {
            return new[] { "false", "true" };
        }

        protected void RemoveAllParametersButNot(params string[] names)
        {
            Parameters = Parameters.Where(param => names.Contains(param.Name)).ToList();
        }

        protected void RemoveDependentParameters(string parentParameterName)
        {
            Parameters = Parameters.Where(param => !param.Name.StartsWith($"{parentParameterName}_")).ToList();
        }

        protected bool IsTrue(ActionParameter parameter)
        {
            return parameter.Value == "true";
        }

        protected IEnumerable<string> GetSelectableTypes()
        {
            var types = new List<string>
            {
                SystemBasicTypes.Binary,
                SystemBasicTypes.IntegralNumber,
                SystemBasicTypes.RealNumber,
                SystemBasicTypes.TextPhrase,
                SystemBasicTypes.DateAndTime
            };
            var dtos = GetAvailableFlowTypesNames();
            types.AddRange(dtos);
            return types;
        }

        protected IEnumerable<ActionResult> GetAvailableLists()
        {
            var lists = new List<ActionResult>();

            IteratePreviousActions(action =>
            {
                var results = action.GetResults();
                foreach (var result in results)
                {
                    if (result.IsVariable && SystemBasicTypes.IsList(result.Type))
                    {
                        lists.Add(result);
                    }
                }
                return true;
            });

            return lists;
        }

        protected IEnumerable<string> GetAvailableListsNames()
        {
            return GetAvailableLists().Select(list => list.Name);
        }
    }
}
