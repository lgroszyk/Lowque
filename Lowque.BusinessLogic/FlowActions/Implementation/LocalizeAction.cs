using Lowque.BusinessLogic.FlowStructure.FlowComponents;
using Lowque.BusinessLogic.SolutionGeneration;
using Lowque.BusinessLogic.Types;
using Lowque.DataAccess;
using Lowque.DataAccess.Internationalization.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Lowque.BusinessLogic.FlowActions.Implementation
{
    class LocalizeAction : BaseAction
    {
        public LocalizeAction(AppDbContext dbContext, ILocalizationContext localizationContext, ISystemTypesContext systemTypesContext) : base(dbContext, localizationContext, systemTypesContext)
        {
        }

        private const string varNameParamName = "LocalizeAction_VarName";
        private const string phraseToLocalizeParamName = "LocalizeAction_PhraseToLocalize";
        private const string argumentsParamName = "LocalizeAction_Arguments";

        private List<ActionParameter> newParams = new List<ActionParameter>();

        public override IEnumerable<string> GetDependencyFields()
        {
            return new[] { DependencyFieldsConsts.LocalizationContext };
        }

        public override PrecompilationValidationResult Validate()
        {
            var hasValidVarNameParam = HasVariableNameParameterValidValue(varNameParamName);
            if (!hasValidVarNameParam)
            {
                return GetVariableNameInvalidError(varNameParamName);
            }

            var hasPhraseToLocalizeParam = HasParameterValue(phraseToLocalizeParamName);
            if (!hasPhraseToLocalizeParam)
            {
                return GetRequiredError(phraseToLocalizeParamName);
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
                Type = SystemBasicTypes.TextPhrase
            };

            return new[] { result };
        }

        public override void GenerateParameters()
        {
            RemoveOtherActionsParameters();

            var varNameParam = AddParameterIfNotExist(varNameParamName, disableHints: true, type: SystemBasicTypes.TextPhrase);
            newParams.Add(varNameParam);

            var phraseToLocalizeParam = AddParameterIfNotExist(phraseToLocalizeParamName, type: SystemBasicTypes.TextPhrase);
            newParams.Add(phraseToLocalizeParam);

            var argumentsParam = AddParameterIfNotExist(argumentsParamName,
                isRequired: false, type: SystemBasicTypes.List, genericType: SystemBasicTypes.TextPhrase);
            newParams.Add(argumentsParam);

            newParams.ForEach(param => param.HasChanged = false);
            Parameters = newParams;
        }

        protected override void GenerateActionSpecificCode()
        {
            var varName = GetParameter(varNameParamName).Value;
            var phraseToLocalize = GetParameter(phraseToLocalizeParamName).Value;

            if (HasParameterValue(argumentsParamName))
            {
                var arguments = GetParameter(argumentsParamName).Value;
                codeLines.Add(new CodeLine($"var {varName} = string.Format(localizationContext.TryTranslate({phraseToLocalize}), {arguments});", CodeIndent.DoNotChange));
            }
            else
            {
                codeLines.Add(new CodeLine($"var {varName} = localizationContext.TryTranslate({phraseToLocalize});", CodeIndent.DoNotChange));
            }
        }
    }
}
