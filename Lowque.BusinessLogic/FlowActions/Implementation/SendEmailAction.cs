using Lowque.BusinessLogic.FlowStructure.FlowComponents;
using Lowque.BusinessLogic.SolutionGeneration;
using Lowque.BusinessLogic.Types;
using Lowque.DataAccess;
using Lowque.DataAccess.Internationalization.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Lowque.BusinessLogic.FlowActions.Implementation
{
    public class SendEmailAction : BaseAction
    {
        public SendEmailAction(AppDbContext dbContext, ILocalizationContext localizationContext, ISystemTypesContext systemTypesContext) : base(dbContext, localizationContext, systemTypesContext) { }

        private const string varNameParamName = "SendEmail_VarName";
        private const string toParamName = "SendEmail_To";
        private const string ccParamName = "SendEmail_Cc";
        private const string subjectParamName = "SendEmail_Subject";
        private const string bodyParamName = "SendEmail_Body";

        private List<ActionParameter> newParams = new List<ActionParameter>();

        public override IEnumerable<string> GetDependencyFields()
        {
            return new[] { DependencyFieldsConsts.EmailSender };
        }

        public override PrecompilationValidationResult Validate()
        {
            if (!HasVariableNameParameterValidValue(varNameParamName))
            {
                return GetVariableNameInvalidError(varNameParamName);
            }

            if (!HasParameterValue(toParamName))
            {
                return GetRequiredError(toParamName);
            }

            if (!HasParameterValue(subjectParamName))
            {
                return GetRequiredError(subjectParamName);
            }

            if (!HasParameterValue(bodyParamName))
            {
                return GetRequiredError(bodyParamName);
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
                Type = SystemBasicTypes.Binary
            };

            return new[] { result };
        }

        public override void GenerateParameters()
        {
            RemoveOtherActionsParameters();

            var varNameParam = AddParameterIfNotExist(varNameParamName, disableHints: true, type: SystemBasicTypes.TextPhrase);
            newParams.Add(varNameParam);

            var toParam = AddParameterIfNotExist(toParamName, type: SystemBasicTypes.List, genericType: SystemBasicTypes.TextPhrase);
            newParams.Add(toParam);

            var ccParam = AddParameterIfNotExist(ccParamName, isRequired: false, type: SystemBasicTypes.List, genericType: SystemBasicTypes.TextPhrase);
            newParams.Add(ccParam);

            var subjectParam = AddParameterIfNotExist(subjectParamName, type: SystemBasicTypes.TextPhrase);
            newParams.Add(subjectParam);

            var bodyParam = AddParameterIfNotExist(bodyParamName, type: SystemBasicTypes.TextPhrase);
            newParams.Add(bodyParam);

            newParams.ForEach(param => param.HasChanged = false);
            Parameters = newParams;
        }

        protected override void GenerateActionSpecificCode()
        {
            var varNameParam = GetParameter(varNameParamName);
            var toParam = GetParameter(toParamName);
            var ccParam = GetParameter(ccParamName);
            var subjectParam = GetParameter(subjectParamName);
            var bodyParam = GetParameter(bodyParamName);

            var toMethodParam = toParam.Value;
            var ccMethodParam = string.IsNullOrEmpty(ccParam.Value) ? "null" : ccParam.Value;
            var subjectMethodParam = subjectParam.Value;
            var bodyMethodParam = bodyParam.Value;

            codeLines.Add(CreateCodeLine($"var {varNameParam.Value} = emailSender.SendEmail({toMethodParam}, {ccMethodParam}, {subjectMethodParam}, {bodyMethodParam});", CodeIndent.DoNotChange));
        }
    }
}
