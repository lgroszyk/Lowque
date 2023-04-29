using Lowque.BusinessLogic.FlowActions;
using Lowque.BusinessLogic.FlowStructure.FlowComponents;
using Lowque.BusinessLogic.FlowStructure.Validation;
using Lowque.BusinessLogic.Types;
using Lowque.DataAccess;
using Lowque.DataAccess.Internationalization.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Lowque.BusinessLogic.FlowStructure
{
    public class FlowContext
    {
        public FlowProperties FlowProperties { get; set; }

        public IEnumerable<BaseAction> Actions { get; set; }

        public IEnumerable<TypeData> Types { get; set; }

        public static FlowContext Create(FlowContent flowContent, FlowProperties flowProperties, 
            AppDbContext dbContext, ILocalizationContext localizationContext, ISystemTypesContext systemTypesContext)
        {
            var flowContext = new FlowContext()
            {
                FlowProperties = flowProperties,
                Types = flowContent.TypesData
            };

            var flowActions = new List<BaseAction>();
            foreach (var nodeData in flowContent.NodesData)
            {
                var previousActions = new List<ActionReference>();
                var previousActionEdges = flowContent.EdgesData.Where(x => x.Target == nodeData.Id);
                foreach (var edge in previousActionEdges)
                {
                    var previousAction = new ActionReference
                    {
                        Id = edge.Source,
                        Condition = edge.Label
                    };
                    previousActions.Add(previousAction);
                }

                var nextActions = new List<ActionReference>();
                var nextActionEdges = flowContent.EdgesData.Where(x => x.Source == nodeData.Id);
                foreach (var edge in nextActionEdges)
                {
                    var nextAction = new ActionReference
                    {
                        Id = edge.Target,
                        Condition = edge.Label
                    };
                    nextActions.Add(nextAction);
                }

                var action = ActionsFactory.GetAction(nodeData.Data.Type, nodeData.Id, nodeData.Data.Label,
                    previousActions, nextActions, nodeData.Data.Parameters.ToList(),
                    dbContext, localizationContext, systemTypesContext);
                if (action != null)
                {
                    action.Context = flowContext;
                    flowActions.Add(action);
                }
            }
            flowContext.Actions = flowActions;

            return flowContext;
        }

        public FlowValidationResult Validate(AppDbContext dbContext, ILocalizationContext localizationContext)
        {
            var edgesCountValidationResult = ValidateEdgesCount(dbContext, localizationContext);
            if (!edgesCountValidationResult.Success)
            {
                return edgesCountValidationResult;
            }

            var areTypesNamesUniqueValidationResult = ValidateAreTypesNamesUnique(dbContext, localizationContext);
            if (!areTypesNamesUniqueValidationResult.Success)
            {
                return areTypesNamesUniqueValidationResult;
            }

            var actionsValidtionResult = ValidateActions();
            if (!actionsValidtionResult.Success)
            {
                return actionsValidtionResult;
            }

            return new FlowValidationResult { Success = true };
        }

        private FlowValidationResult ValidateEdgesCount(AppDbContext dbContext, ILocalizationContext localizationContext)
        {
            if (!Actions.Any())
            {
                return new FlowValidationResult
                {
                    Success = true,
                };
            }

            var actionsOfKindStart = Actions.Where(x =>
                x.PreviousActions.Count() == 0 &&
                x.NextActions.Count() == 1);
            if (actionsOfKindStart.Count() != 1)
            {
                return new FlowValidationResult
                {
                    Success = false,
                    Message = localizationContext.TryTranslate("FlowDesigner_FlowValidation_MustHaveOnlyOneStartAction")
                };
            }

            var actionsOfKindEnd = Actions.Where(x => 
                x.Type == "Return" && 
                x.PreviousActions.Count() == 1 &&
                x.NextActions.Count() == 0);
            if (actionsOfKindEnd.Count() != 1)
            {
                return new FlowValidationResult
                {
                    Success = false,
                    Message = localizationContext.TryTranslate("FlowDesigner_FlowValidation_MustHaveOnlyOneEndAction")
                };
            }

            var actionsOfTypeReturn = Actions.Where(x => x.Type == "Return");
            if (actionsOfKindEnd.Count() != 1)
            {
                return new FlowValidationResult
                {
                    Success = false,
                    Message = localizationContext.TryTranslate("FlowDesigner_FlowValidation_MustHaveOnlyOneReturnAction")
                };

            }

            var actionOfTypeIfElseStart = Actions.Where(x => x.Type == "IfElseStart");
            if (actionOfTypeIfElseStart.Any(action =>
                action.PreviousActions.Count() != 1 ||
                action.NextActions.Count() != 2))
            {
                return new FlowValidationResult
                {
                    Success = false,
                    Message = localizationContext.TryTranslate("FlowDesigner_FlowValidation_IfElseStartMustHaveOnePreviousActionAndTwoNextActions")
                };
            }

            var actionOfTypeIfElseEnd = Actions.Where(x => x.Type == "IfElseEnd");
            if (actionOfTypeIfElseEnd.Any(action =>
                action.PreviousActions.Count() != 2 ||
                action.NextActions.Count() != 1))
            {
                return new FlowValidationResult
                {
                    Success = false,
                    Message = localizationContext.TryTranslate("FlowDesigner_FlowValidation_IfElseEndMustHaveTwoPreviousActionsAndOneNextAction")
                };
            }

            var otherActions = Actions.Where(x =>
                !actionsOfKindStart.Contains(x) &&
                !actionsOfKindEnd.Contains(x) &&
                !actionsOfTypeReturn.Contains(x) &&
                !actionOfTypeIfElseStart.Contains(x) &&
                !actionOfTypeIfElseEnd.Contains(x));
            if (otherActions.Any(action => action.NextActions.Count() != 1 && action.PreviousActions.Count() != 1))
            {
                return new FlowValidationResult
                {
                    Success = false,
                    Message = localizationContext.TryTranslate("FlowDesigner_FlowValidation_StandardMustHaveOnePreviousActionAndOneNextAction")
                };
            }

            return new FlowValidationResult
            {
                Success = true,
            };
        }

        private FlowValidationResult ValidateAreTypesNamesUnique(AppDbContext dbContext, ILocalizationContext localizationContext)
        {
            var flowsInSameArea = dbContext.FlowDefinitions
                .Where(flow => flow.Area == FlowProperties.FlowArea && flow.Name != FlowProperties.FlowName);
            foreach (var flow in flowsInSameArea)
            {
                var typesNames = JsonConvert.DeserializeObject<FlowContent>(flow.Content).TypesData
                    .Select(type => type.Name);

                var checkedFlowTypesNames = Types.Select(type => type.Name);
                var checkedFlowDuplicates = typesNames.Intersect(checkedFlowTypesNames);
                var checkedFlowHasDuplicates = checkedFlowDuplicates.Any();
                if (checkedFlowHasDuplicates)
                {
                    return new FlowValidationResult
                    {
                        Success = false,
                        Message = localizationContext.TryTranslate("FlowDesigner_FlowValidation_TypeNameIsDuplicate", checkedFlowDuplicates.First())
                    };
                }
            }

            return new FlowValidationResult
            {
                Success = true
            };
        }

        private FlowValidationResult ValidateActions()
        {
            var actionsValidationResult = Actions.Select(action => action.Validate());
            var firstInvalidAction = actionsValidationResult.FirstOrDefault(action => !action.IsValid);
            if (firstInvalidAction != null)
            {
                return new FlowValidationResult
                {
                    Success = false,
                    Message = firstInvalidAction.Errors.First()
                };
            }
            return new FlowValidationResult
            {
                Success = true
            };
        }
    }
}
