using Lowque.BusinessLogic.FlowActions.Implementation;
using Lowque.BusinessLogic.FlowStructure.FlowComponents;
using Lowque.BusinessLogic.Types;
using Lowque.DataAccess;
using Lowque.DataAccess.Internationalization.Interfaces;
using System.Collections.Generic;

namespace Lowque.BusinessLogic.FlowActions
{
    public static class ActionsFactory
    {
        public static BaseAction GetAction(
            string type, 
            string id, 
            string name,
            IEnumerable<ActionReference> previousActions,
            IEnumerable<ActionReference> nextActions,
            List<ActionParameter> parameters,
            AppDbContext dbContext,
            ILocalizationContext localizationContext,
            ISystemTypesContext systemTypesContext)
        {
            BaseAction action;
            switch (type)
            {
                case "CreateVariable": action = new CreateVariableAction(dbContext, localizationContext, systemTypesContext); break;
                case "ChangeVariable": action = new ChangeVariableAction(dbContext, localizationContext, systemTypesContext); break;
                case "IfElseStart": action = new IfElseStartAction(dbContext, localizationContext, systemTypesContext); break; 
                case "IfElseEnd": action = new IfElseEndAction(dbContext, localizationContext, systemTypesContext); break;
                case "LoopStart": action = new LoopStartAction(dbContext, localizationContext, systemTypesContext); break;
                case "LoopEnd": action = new LoopEndAction(dbContext, localizationContext, systemTypesContext); break;
                case "Return": action = new ReturnAction(dbContext, localizationContext, systemTypesContext); break;
                case "AddRecord": action = new AddRecordAction(dbContext, localizationContext, systemTypesContext); break;
                case "EditRecord": action = new EditRecordAction(dbContext, localizationContext, systemTypesContext); break;
                case "DeleteRecord": action = new DeleteRecordAction(dbContext, localizationContext, systemTypesContext); break;
                case "GetRecords": action = new GetRecordsAction(dbContext, localizationContext, systemTypesContext); break;
                case "AddElement": action = new AddElementAction(dbContext, localizationContext, systemTypesContext); break;
                case "DeleteElement": action = new DeleteElementAction(dbContext, localizationContext, systemTypesContext); break;
                case "GetElements": action = new GetElementsAction(dbContext, localizationContext, systemTypesContext); break;
                case "GetCurrentUser": action = new GetCurrentUserAction(dbContext, localizationContext, systemTypesContext); break;
                case "UploadDocument": action = new UploadDocumentAction(dbContext, localizationContext, systemTypesContext); break;
                case "DeleteDocument": action = new DeleteDocumentAction(dbContext, localizationContext, systemTypesContext); break;
                case "GetDocumentPath": action = new GetDocumentPathAction(dbContext, localizationContext, systemTypesContext); break;
                case "SendEmail": action = new SendEmailAction(dbContext, localizationContext, systemTypesContext); break;
                case "Localize": action = new LocalizeAction(dbContext, localizationContext, systemTypesContext); break;
                default: return null;
            }
            action.Type = type;
            action.Id = id;
            action.Name = name;
            action.PreviousActions = previousActions;
            action.NextActions = nextActions;
            action.Parameters = parameters;

            return action;
        }
    }
}
