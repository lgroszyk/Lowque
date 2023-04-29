﻿using Lowque.BusinessLogic.FlowStructure.FlowComponents;
using Lowque.BusinessLogic.SolutionGeneration;
using Lowque.BusinessLogic.Types;
using Lowque.DataAccess;
using Lowque.DataAccess.Internationalization.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Lowque.BusinessLogic.FlowActions.Implementation
{
    public class AddRecordAction : BaseAction
    {
        public AddRecordAction(AppDbContext dbContext, ILocalizationContext localizationContext, ISystemTypesContext systemTypesContext) 
            : base(dbContext, localizationContext, systemTypesContext) { }

        private const string varNameParamName = "AddRecord_VarName";
        private const string tableParamName = "AddRecord_Table";
        private const string propParamNamePrefix = "AddRecord_Property_";

        private List<ActionParameter> newParams = new List<ActionParameter>();

        public override IEnumerable<string> GetDependencyFields()
        {
            return new[] { DependencyFieldsConsts.AppDbContext, DependencyFieldsConsts.EntityValidator };
        }

        public override PrecompilationValidationResult Validate()
        {
            if (!HasVariableNameParameterValidValue(varNameParamName))
            {
                return GetVariableNameInvalidError(varNameParamName);
            }

            if (!HasParameterValidValue(tableParamName, GetAvailableDatabaseTypesNames()))
            {
                return GetInvalidValueError(tableParamName);
            }

            return PrecompilationValidationResult.Valid();
        }

        public override IEnumerable<ActionResult> GetResults()
        {
            if (!HasParameterValue(varNameParamName) || !HasParameterValue(tableParamName))
            {
                return Enumerable.Empty<ActionResult>();
            }

            var varName = GetParameter(varNameParamName).Value;
            var varType = GetParameter(tableParamName).Value;
            var newEntityResult = new ActionResult
            {
                Name = varName,
                Type = varType,
                Properties = GetComplexTypeActionResultProperties(varType)
            };

            var validationResult = new ActionResult
            {
                Name = $"{varName}ValidationResult",
                Type = "EntityValidationResult",
                Properties = GetComplexTypeActionResultProperties("EntityValidationResult")
            };

            return new[] { newEntityResult, validationResult };
        }

        protected override void GenerateActionSpecificCode()
        {
            var varName = GetParameter(varNameParamName).Value;
            var varType = GetParameter(tableParamName).Value;

            codeLines.Add(CreateCodeLine($"var {varName} = new {varType}", CodeIndent.DoNotChange));
            codeLines.Add(CreateCodeLine("{", CodeIndent.AddAfter));

            var propsParams = GetParametersStartingWith(propParamNamePrefix).Where(prop => !string.IsNullOrEmpty(prop.Value));
            foreach (var propParam in propsParams)
            {
                var propName = propParam.Name.Split(propParamNamePrefix).ElementAt(1);
                var propValue = propParam.Value;
                codeLines.Add(CreateCodeLine($"{propName} = {propValue},", CodeIndent.DoNotChange));
            }
            codeLines.Add(CreateCodeLine("};", CodeIndent.DeleteBefore));

            codeLines.Add(CreateCodeLine($"var {varName}ValidationResult = entityValidator.Validate({varName});", CodeIndent.DoNotChange));
            codeLines.Add(CreateCodeLine($"if ({varName}ValidationResult.IsValid)", CodeIndent.DoNotChange));
            codeLines.Add(CreateCodeLine("{", CodeIndent.AddAfter));
            codeLines.Add(CreateCodeLine($"dbContext.Add({varName});", CodeIndent.DoNotChange));
            codeLines.Add(CreateCodeLine("dbContext.SaveChanges();", CodeIndent.DoNotChange));
            codeLines.Add(CreateCodeLine("}", CodeIndent.DeleteBefore));
            codeLines.Add(CreateCodeLine("", CodeIndent.DoNotChange));
        }

        public override void GenerateParameters()
        {
            RemoveOtherActionsParameters();

            var varNameParam = AddParameterIfNotExist(varNameParamName, disableHints: true, type: SystemBasicTypes.TextPhrase);
            newParams.Add(varNameParam);

            var tableParam = AddParameterIfNotExist(tableParamName, GetAvailableDatabaseTypesNames());
            newParams.Add(tableParam);
            if (HasParameterChanged(tableParamName))
            {
                RemoveAllParametersButNot(varNameParamName, tableParamName);
            }

            var editableProps = GetComplexTypeProperties(tableParam.Value)
                .Where(prop => !prop.Key && !prop.Navigation);
            foreach (var prop in editableProps)
            {
                var propParam = AddParameterIfNotExist($"{propParamNamePrefix}{prop.Name}", 
                    disableTranslating: true, isRequired: false, type: prop.ActionType, genericType: prop.ActionGenericType);
                newParams.Add(propParam);
            }

            newParams.ForEach(param => param.HasChanged = false);
            Parameters = newParams;
        }
    }
}
