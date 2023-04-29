using Lowque.BusinessLogic.Dto.FlowDesigner;
using Lowque.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lowque.WebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FlowDesignerController
    {
        private readonly IFlowDesignerService designerService;

        public FlowDesignerController(IFlowDesignerService designerService)
        {
            this.designerService = designerService;
        }

        [HttpGet("GetFlow/{id}")]
        public GetFlowResponseDto GetFlow(int id)
        {
            return designerService.GetFlow(id);
        }

        [HttpPost("SaveFlow")]
        public SaveFlowResponseDto SaveFlow(SaveFlowRequestDto dto)
        {
            return designerService.SaveFlow(dto);
        }

        [HttpPost("GetParameters")]
        public GetParametersResponseDto GetParameters(GetParametersRequestDto dto)
        {
            return designerService.GetParameters(dto);
        }

        [HttpPost("CreateNewAction")]
        public CreateNewActionResponseDto CreateNewAction(CreateNewActionRequestDto dto)
        {
            return designerService.CreateNewAction(dto);
        }

        [HttpPost("ModifyDtosDueToTypeChange")]
        public ModifyDtosDueToTypeChangeResponseDto ModifyDtosDueToTypeChange(ModifyDtosDueToTypeChangeRequestDto dto)
        {
            return designerService.ModifyDtosDueToTypeChange(dto);
        }
    }
}
