using Lowque.BusinessLogic.SolutionGeneration;
using Lowque.BusinessLogic.Types;
using Lowque.DataAccess;
using Lowque.DataAccess.Internationalization.Interfaces;
using System.Linq;

namespace Lowque.BusinessLogic.FlowActions.Implementation
{
    public class IfElseStartAction : BaseAction
    {
        public IfElseStartAction(AppDbContext dbContext, ILocalizationContext localizationContext, ISystemTypesContext systemTypesContext) : base(dbContext, localizationContext, systemTypesContext) { }

        private const string conditionParamName = "IfElseStart_Condition";

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
            codeLines.Add(CreateCodeLine($"if ({conditionParam.Value})", CodeIndent.DoNotChange));

            var firstTrueAction = TryGetActionFromReference(NextActions.ElementAt(0));
            var truePathCode = firstTrueAction.GenerateCode();
            codeLines.Add(CreateCodeLine("{", CodeIndent.AddAfter));
            codeLines.AddRange(truePathCode);
            codeLines.Add(CreateCodeLine("}", CodeIndent.DeleteBefore));

            var firstFalseAction = TryGetActionFromReference(NextActions.ElementAt(1));
            var falsePathCode = firstFalseAction.GenerateCode();
            if (falsePathCode.Any())
            {
                codeLines.Add(CreateCodeLine("else", CodeIndent.DoNotChange));
                codeLines.Add(CreateCodeLine("{", CodeIndent.AddAfter));
                codeLines.AddRange(falsePathCode);
                codeLines.Add(CreateCodeLine("}", CodeIndent.DeleteBefore));
            }
        }

        protected override BaseAction TryFindNextAction()
        {
            var nextAction = Context.Actions.SingleOrDefault(x => 
                x.PreviousActions.Any() && 
                x.PreviousActions.ElementAt(0).Id == TryFindNextActionOfType("IfElseEnd")?.Id);
            return nextAction;
        }
    }
}
