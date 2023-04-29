using Lowque.BusinessLogic.FlowStructure.FlowComponents;
using Lowque.BusinessLogic.SolutionGeneration;
using Lowque.BusinessLogic.Types;
using Lowque.DataAccess;
using Lowque.DataAccess.Internationalization.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Lowque.BusinessLogic.FlowActions.Implementation
{
    public class AddElementAction : BaseAction
    {
        public AddElementAction(AppDbContext dbContext, ILocalizationContext localizationContext, ISystemTypesContext systemTypesContext) : base(dbContext, localizationContext, systemTypesContext) 
        {
        }

        private const string collectionParamName = "AddElementAction_Collection";
        private const string singleParamName = "AddElementAction_Single";
        private const string elementParamName = "AddElementAction_Element";

        private List<ActionParameter> newParams = new List<ActionParameter>();

        public override PrecompilationValidationResult Validate()
        {
            
            var hasValidCollectionParam = HasParameterValidValue(collectionParamName, GetAvailableListsNames());
            if (!hasValidCollectionParam)
            {
                return GetInvalidValueError(collectionParamName);
            }

            var isSingleParamValid = HasParameterValidValue(singleParamName, GetBooleanValues());
            if (!isSingleParamValid)
            {
                return GetInvalidValueError(singleParamName);
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

            var singleParam = AddParameterIfNotExist(singleParamName, GetBooleanValues());
            newParams.Add(singleParam);
            if (HasParameterChanged(singleParamName))
            {
                RemoveParameterIfExist(elementParamName);
            }

            var chosenList = GetAvailableLists().FirstOrDefault(list => list.Name == collectionParam.Value);
            var elementParam = AddParameterIfNotExist(elementParamName, type: chosenList?.GenericType);
            newParams.Add(elementParam);

            newParams.ForEach(param => param.HasChanged = false);
            Parameters = newParams;
        }

        protected override void GenerateActionSpecificCode()
        {
            var collection = GetParameter(collectionParamName).Value;
            var element = GetParameter(elementParamName).Value;

            if (IsTrue(GetParameter(singleParamName)))
            {
                codeLines.Add(CreateCodeLine($"{collection}.Add({element});", CodeIndent.DoNotChange));
            }
            else
            {
                codeLines.Add(CreateCodeLine($"{collection}.AddRange(new [] {{ {element} }});", CodeIndent.DoNotChange));
            }
        }
    }
}
