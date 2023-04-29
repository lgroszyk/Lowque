using Lowque.BusinessLogic.Dto.FlowDesigner;

namespace Lowque.BusinessLogic.Services.Interfaces
{
    public interface IFlowDesignerService
    {
        GetFlowResponseDto GetFlow(int id);
        SaveFlowResponseDto SaveFlow(SaveFlowRequestDto dto);
        GetParametersResponseDto GetParameters(GetParametersRequestDto dto);
        CreateNewActionResponseDto CreateNewAction(CreateNewActionRequestDto dto);
        ModifyDtosDueToTypeChangeResponseDto ModifyDtosDueToTypeChange(ModifyDtosDueToTypeChangeRequestDto dto);
    }
}
