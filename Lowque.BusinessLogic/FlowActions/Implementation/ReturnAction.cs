using Lowque.BusinessLogic.SolutionGeneration;
using Lowque.BusinessLogic.Types;
using Lowque.DataAccess;
using Lowque.DataAccess.Internationalization.Interfaces;

namespace Lowque.BusinessLogic.FlowActions.Implementation
{
    public class ReturnAction : BaseAction
    {
        public ReturnAction(AppDbContext dbContext, ILocalizationContext localizationContext, ISystemTypesContext systemTypesContext) : base(dbContext, localizationContext, systemTypesContext) { }

        private const string valueParamName = "Return_Value";

        protected override void GenerateActionSpecificCode()
        {
            codeLines.Add(CreateCodeLine($"return {Parameters[0].Value};", CodeIndent.DoNotChange));
        }

        public override PrecompilationValidationResult Validate()
        {
            if (!HasParameterValue(valueParamName))
            {
                return GetRequiredError(valueParamName);
            }

            return PrecompilationValidationResult.Valid();
        }

        protected override void GenerateNextActionsCode()
        {
            
        }

        public override void GenerateParameters()
        {
            RemoveOtherActionsParameters();
            AddParameterIfNotExist(valueParamName, type: $"{this.Context.FlowProperties.FlowName}ResponseDto");            
        }
    }
}
