using Lowque.BusinessLogic.SolutionGeneration;
using Lowque.BusinessLogic.Types;
using Lowque.DataAccess;
using Lowque.DataAccess.Internationalization.Interfaces;
using System.Linq;

namespace Lowque.BusinessLogic.FlowActions.Implementation
{
    public class LoopStartAction : BaseAction
    {
        public LoopStartAction(AppDbContext dbContext, ILocalizationContext localizationContext, ISystemTypesContext systemTypesContext) : base(dbContext, localizationContext, systemTypesContext) { }

        private const string conditionParamName = "LoopStart_Condition";

        public override PrecompilationValidationResult Validate()
        {
            if (!HasParameterValue(conditionParamName))
            {
                return GetRequiredError(conditionParamName);
            }

            return PrecompilationValidationResult.Valid();
        }

        public override void GenerateParameters()
        {
            RemoveOtherActionsParameters();
            AddParameterIfNotExist(conditionParamName, type: SystemBasicTypes.Binary);
        }

        protected override void GenerateActionSpecificCode()
        {
            var conditionParam = GetParameter(conditionParamName);
            codeLines.Add(CreateCodeLine($"while ({conditionParam.Value})", CodeIndent.DoNotChange));

            var firstLoopBodyAction = TryGetActionFromReference(NextActions.ElementAt(0));
            var loopBodyCode = firstLoopBodyAction.GenerateCode();
            codeLines.Add(CreateCodeLine("{", CodeIndent.AddAfter));
            codeLines.AddRange(loopBodyCode);
            codeLines.Add(CreateCodeLine("}", CodeIndent.DeleteBefore));
        }

        protected override BaseAction TryFindNextAction()
        {
            var nextAction = Context.Actions.SingleOrDefault(x =>
                x.PreviousActions.Any() &&
                x.PreviousActions.ElementAt(0).Id == TryFindNextActionOfType("LoopEnd")?.Id);
            return nextAction;
        }
    }
}
