using Lowque.BusinessLogic.FlowStructure.FlowComponents;
using Lowque.BusinessLogic.SolutionGeneration;
using Lowque.BusinessLogic.Types;
using Lowque.DataAccess;
using Lowque.DataAccess.Internationalization.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Lowque.BusinessLogic.FlowActions.Implementation
{
    public class DeleteElementAction : BaseAction
    {
        public DeleteElementAction(AppDbContext dbContext, ILocalizationContext localizationContext, ISystemTypesContext systemTypesContext) : base(dbContext, localizationContext, systemTypesContext)
        {
        }

        private const string collectionParamName = "DeleteElement_Collection";
        private const string elementParamName = "DeleteElement_Element";

        private List<ActionParameter> newParams = new List<ActionParameter>();

        public override PrecompilationValidationResult Validate()
        {
            var hasValidCollectionParam = HasParameterValidValue(collectionParamName, GetAvailableListsNames());
            if (!hasValidCollectionParam)
            {
                return GetRequiredError(collectionParamName);
            }

            var hasElementParam = HasParameterValue(elementParamName);
            if (!hasElementParam)
            {
                return GetRequiredError(elementParamName);
            }

            return PrecompilationValidationResult.Valid();
        }

        public override void GenerateParameters()
        {
            RemoveOtherActionsParameters();

            var collectionParam = AddParameterIfNotExist(collectionParamName, availableValues: GetAvailableListsNames());
            newParams.Add(collectionParam);
            if (HasParameterChanged(collectionParamName))
            {
                RemoveParameterIfExist(elementParamName);
            }

            var chosenList = GetAvailableLists().FirstOrDefault(list => list.Name == collectionParam.Value);
            var elementParam = AddParameterIfNotExist(elementParamName, type: chosenList?.Type, genericType: chosenList?.GenericType);
            newParams.Add(elementParam);

            newParams.ForEach(param => param.HasChanged = false);
            Parameters = newParams;
        }

        protected override void GenerateActionSpecificCode()
        {
            var collectionParam = GetParameter(collectionParamName);
            var elementParam = GetParameter(elementParamName);

            codeLines.Add(CreateCodeLine($"{collectionParam.Value}.Remove({elementParam.Value});", CodeIndent.DoNotChange));
        }
    }
}
