using Lowque.BusinessLogic.FlowStructure.FlowComponents;
using Lowque.BusinessLogic.SolutionGeneration;
using Lowque.BusinessLogic.Types;
using Lowque.DataAccess;
using Lowque.DataAccess.Internationalization.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Lowque.BusinessLogic.FlowActions.Implementation
{
    public class GetCurrentUserAction : BaseAction
    {
        public GetCurrentUserAction(AppDbContext dbContext, ILocalizationContext localizationContext, ISystemTypesContext systemTypesContext) : base(dbContext, localizationContext, systemTypesContext) { }

        private const string varNameParamName = "GetCurrentUser_VarName";

        private List<ActionParameter> newParams = new List<ActionParameter>();

        public override IEnumerable<string> GetDependencyFields()
        {
            return new[] { DependencyFieldsConsts.UserContext };
        }

        public override PrecompilationValidationResult Validate()
        {
            if (!HasVariableNameParameterValidValue(varNameParamName))
            {
                return GetVariableNameInvalidError(varNameParamName);
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
                Type = "CurrentUserData",
                Properties = GetComplexTypeActionResultProperties("CurrentUserData")
            };

            return new[] { result };
        }

        public override void GenerateParameters()
        {
            RemoveOtherActionsParameters();

            var varNameParam = AddParameterIfNotExist(varNameParamName, disableHints: true, type: SystemBasicTypes.TextPhrase);
            newParams.Add(varNameParam);

            newParams.ForEach(param => param.HasChanged = false);
            Parameters = newParams;
        }

        protected override void GenerateActionSpecificCode()
        {
            var varNameParam = GetParameter(varNameParamName);
            codeLines.Add(CreateCodeLine($"var {varNameParam.Value} = userContext.GetCurrentUserData();", CodeIndent.DoNotChange));
        }
    }
}
