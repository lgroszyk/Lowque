using Lowque.BusinessLogic.Types;
using Lowque.DataAccess;
using Lowque.DataAccess.Internationalization.Interfaces;

namespace Lowque.BusinessLogic.FlowActions.Implementation
{
    public class IfElseEndAction : BaseAction
    {
        public IfElseEndAction(AppDbContext dbContext, ILocalizationContext localizationContext, ISystemTypesContext systemTypesContext) : base(dbContext, localizationContext, systemTypesContext) { }

        protected override void GenerateActionSpecificCode()
        {
            
        }

        protected override void GenerateNextActionsCode()
        {
           
        }

        public override void GenerateParameters()
        {
            RemoveOtherActionsParameters();
        }
    }
}
