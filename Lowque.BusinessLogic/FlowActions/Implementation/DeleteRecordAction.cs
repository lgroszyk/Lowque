using Lowque.BusinessLogic.FlowStructure.FlowComponents;
using Lowque.BusinessLogic.SolutionGeneration;
using Lowque.BusinessLogic.Types;
using Lowque.DataAccess;
using Lowque.DataAccess.Internationalization.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Lowque.BusinessLogic.FlowActions.Implementation
{
    class DeleteRecordAction : BaseAction
    {
        public DeleteRecordAction(AppDbContext dbContext, ILocalizationContext localizationContext, ISystemTypesContext systemTypesContext) : base(dbContext, localizationContext, systemTypesContext) { }

        private const string varNameParamName = "DeleteRecord_VarName";
        private const string sourceParamName = "DeleteRecord_Source";
        private const string keyParamName = "DeleteRecord_Key";

        public override IEnumerable<string> GetDependencyFields()
        {
            return new[] { DependencyFieldsConsts.AppDbContext };
        }

        public override PrecompilationValidationResult Validate()
        {
            var hasValidVarNameParam = HasVariableNameParameterValidValue(varNameParamName);
            if (!hasValidVarNameParam)
            {
                return GetVariableNameInvalidError(varNameParamName);
            }

            var hasValidSourceParam = HasParameterValidValue(sourceParamName, GetAvailableDatabaseTypesNames());
            if (!hasValidSourceParam)
            {
                return GetInvalidValueError(sourceParamName);
            }

            var hasKeyParam = HasParameterValue(keyParamName);
            if (!hasKeyParam)
            {
                return GetRequiredError(keyParamName);
            }

            return PrecompilationValidationResult.Valid();
        }

        public override IEnumerable<ActionResult> GetResults()
        {
            if (!HasParameterValue(varNameParamName) || !HasParameterValue(sourceParamName))
            {
                return Enumerable.Empty<ActionResult>();
            }

            var varNameParam = GetParameter(varNameParamName);
            var sourceParam = GetParameter(sourceParamName);
            var varResult = new ActionResult
            {
                Name = varNameParam.Value,
                Type = sourceParam.Value,
                Properties = GetComplexTypeActionResultProperties(sourceParam.Value)
            };

            return new[] { varResult };
        }

        public override void GenerateParameters()
        {
            RemoveOtherActionsParameters();
            var newParams = new List<ActionParameter>();

            var varNameParam = AddParameterIfNotExist(varNameParamName, disableHints: true, type: SystemBasicTypes.TextPhrase);
            newParams.Add(varNameParam);

            var sourceParam = AddParameterIfNotExist(sourceParamName, GetAvailableDatabaseTypesNames());
            newParams.Add(sourceParam);

            var keyPropParam = AddParameterIfNotExist(keyParamName, type: SystemBasicTypes.IntegralNumber);
            newParams.Add(keyPropParam);

            newParams.ForEach(param => param.HasChanged = false);
            Parameters = newParams;
        }

        protected override void GenerateActionSpecificCode()
        {
            var varNameParam = GetParameter(varNameParamName);
            var sourceParam = GetParameter(sourceParamName);
            var keyParam = GetParameter(keyParamName);

            codeLines.Add(CreateCodeLine($"var {varNameParam.Value} = dbContext.Set<{sourceParam.Value}>().Find({keyParam.Value});", CodeIndent.DoNotChange));
            codeLines.Add(CreateCodeLine($"dbContext.Set<{sourceParam.Value}>().Remove({varNameParam.Value});", CodeIndent.DoNotChange));
            codeLines.Add(CreateCodeLine($"dbContext.SaveChanges();", CodeIndent.DoNotChange));
            codeLines.Add(CreateCodeLine($"", CodeIndent.DoNotChange));
        }
    }
}
