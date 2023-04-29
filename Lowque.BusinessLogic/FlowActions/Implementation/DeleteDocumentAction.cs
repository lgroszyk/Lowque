using Lowque.BusinessLogic.FlowStructure.FlowComponents;
using Lowque.BusinessLogic.SolutionGeneration;
using Lowque.BusinessLogic.Types;
using Lowque.DataAccess;
using Lowque.DataAccess.Internationalization.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Lowque.BusinessLogic.FlowActions.Implementation
{
    public class DeleteDocumentAction : BaseAction
    {
        public DeleteDocumentAction(AppDbContext dbContext, ILocalizationContext localizationContext, ISystemTypesContext systemTypesContext) : base(dbContext, localizationContext, systemTypesContext) { }

        private const string varNameParamName = "DeleteDocument_VarName";
        private const string directoryNameParamName = "DeleteDocument_DirectoryName";
        private const string fileNameParamName = "DeleteDocument_FileName";

        private List<ActionParameter> newParams = new List<ActionParameter>();

        public override IEnumerable<string> GetDependencyFields()
        {
            return new[] { DependencyFieldsConsts.DocumentContext };
        }

        public override PrecompilationValidationResult Validate()
        {
            if (!HasVariableNameParameterValidValue(varNameParamName))
            {
                return GetVariableNameInvalidError(varNameParamName);
            }

            if (!HasParameterValue(directoryNameParamName))
            {
                return GetRequiredError(directoryNameParamName);
            }

            if (!HasParameterValue(fileNameParamName))
            {
                return GetRequiredError(fileNameParamName);
            }

            return PrecompilationValidationResult.Valid();
        }

        public override IEnumerable<ActionResult> GetResults()
        {
            if (!HasParameterValue(varNameParamName))
            {
                return Enumerable.Empty<ActionResult>();
            }

            var result = new ActionResult
            {
                Name = GetParameter(varNameParamName).Value,
                Type = "DeleteDocumentResult",
                Properties = GetComplexTypeActionResultProperties("DeleteDocumentResult")
            };

            return new[] { result };
        }

        public override void GenerateParameters()
        {
            RemoveOtherActionsParameters();

            var varNameParam = AddParameterIfNotExist(varNameParamName, disableHints: true, type: SystemBasicTypes.TextPhrase);
            newParams.Add(varNameParam);

            var directoryNameParam = AddParameterIfNotExist(directoryNameParamName, type: SystemBasicTypes.TextPhrase);
            newParams.Add(directoryNameParam);

            var fileNameParam = AddParameterIfNotExist(fileNameParamName, type: SystemBasicTypes.TextPhrase);
            newParams.Add(fileNameParam);

            newParams.ForEach(param => param.HasChanged = false);
            Parameters = newParams;
        }

        protected override void GenerateActionSpecificCode()
        {
            var varNameParam = GetParameter(varNameParamName);
            var directoryNameParam = GetParameter(directoryNameParamName);
            var fileNameParam = GetParameter(fileNameParamName);

            codeLines.Add(CreateCodeLine($"var {varNameParam.Value} = documentContext.DeleteDocument({directoryNameParam.Value}, {fileNameParam.Value});", CodeIndent.DoNotChange));
        }
    }
}
