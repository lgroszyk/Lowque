using System;
using System.Collections.Generic;
using System.Linq;
using Lowque.BusinessLogic.Dto.FlowDesigner;
using Lowque.BusinessLogic.FlowStructure;
using Lowque.BusinessLogic.FlowStructure.FlowComponents;
using Lowque.BusinessLogic.Services.Interfaces;
using Lowque.BusinessLogic.SolutionCompilation;
using Lowque.BusinessLogic.SolutionGeneration;
using Lowque.BusinessLogic.Types;
using Lowque.DataAccess;
using Lowque.DataAccess.Entities;
using Lowque.DataAccess.Internationalization.Interfaces;
using Lowque.DataAccess.SolutionCompilation;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Lowque.BusinessLogic
{
    public class FlowDesignerService : IFlowDesignerService
    {
        private readonly AppDbContext dbContext;
        private readonly ISystemTypesContext systemTypeContext;
        private readonly ISolutionGenerator solutionGenerator;
        private readonly ISolutionCompilator solutionCompilator;
        private readonly ICompilationErrorsFormatter compilationErrorsFormatter;
        private readonly ICompilationFilesCleaner compilationFilesCleaner;
        private readonly ILocalizationContext localizationContext;

        public FlowDesignerService(AppDbContext dbContext, 
            ISystemTypesContext systemTypeContext,
            ISolutionGenerator solutionGenerator,
            ISolutionCompilator solutionCompilator,
            ICompilationErrorsFormatter compilationErrorsFormatter,
            ICompilationFilesCleaner compilationFilesCleaner,
            ILocalizationContext localizationContext)
        {
            this.dbContext = dbContext;
            this.systemTypeContext = systemTypeContext;
            this.solutionGenerator = solutionGenerator;
            this.solutionCompilator = solutionCompilator;
            this.compilationErrorsFormatter = compilationErrorsFormatter;
            this.compilationFilesCleaner = compilationFilesCleaner;
            this.localizationContext = localizationContext;
        }

        public GetFlowResponseDto GetFlow(int id)
        {
            var flow = dbContext.FlowDefinitions
                .Include(flow => flow.ApplicationDefinition)
                .Single(flow => flow.FlowDefinitionId == id);
            var flowData = JsonConvert.DeserializeObject<FlowContent>(flow.Content);
            var flowProperties = new FlowProperties
            {
                AppId = flow.ApplicationDefinitionId,
                AppName = flow.ApplicationDefinition.Name,
                FlowId = flow.FlowDefinitionId,
                FlowName = flow.Name,
                FlowArea = flow.Area,
                FlowType = flow.Type,
                UseResourceIdentifier = flow.UseResourceIdentifier
            };

            return new GetFlowResponseDto
            {
                FlowData = flowData,
                FlowProperties = flowProperties
            };
        }

        public SaveFlowResponseDto SaveFlow(SaveFlowRequestDto dto)
        {
            var flow = dbContext.FlowDefinitions
                .Include(flow => flow.ApplicationDefinition)
                .Single(flow => flow.FlowDefinitionId == dto.FlowProperties.FlowId);
            flow.Content = JsonConvert.SerializeObject(dto.FlowData);
            flow.Type = dto.FlowProperties.FlowType;
            flow.UseResourceIdentifier = dto.FlowProperties.UseResourceIdentifier;
            dbContext.SaveChanges();

            var sandboxApplicationDefinition = new ApplicationDefinition()
            {
                ApplicationDefinitionId = flow.ApplicationDefinitionId,
                Name = flow.ApplicationDefinition.Name,
                Template = flow.ApplicationDefinition.Template,
                IsTemplate = false,
                CreatedAt = flow.ApplicationDefinition.CreatedAt,
                BusinessLogicModuleDefinition = new List<FlowDefinition> { flow }
            };

            try
            {
                var filesGenerationResult = solutionGenerator.Generate(sandboxApplicationDefinition, new SolutionOptions(SolutionType.Temporary));
                if (filesGenerationResult.FlowGenerationErrors.Any())
                {
                    return new SaveFlowResponseDto
                    {
                        Success = false,
                        Error = filesGenerationResult.FlowGenerationErrors.First().ErrorMessage
                    };
                }

                var compilationResult = solutionCompilator.Compile(sandboxApplicationDefinition.Name, new SolutionOptions(SolutionType.Temporary));
                var compilationResultErrors = compilationErrorsFormatter.Format(filesGenerationResult, compilationResult);
                if (compilationResultErrors.Any())
                {
                    var errorToDisplay = compilationResultErrors.First();
                    var errorMessage = string.IsNullOrEmpty(errorToDisplay.ActionName)
                        ? errorToDisplay.Error
                        : $"{compilationResultErrors.First().ActionName}: {compilationResultErrors.First().Error}";
                    return new SaveFlowResponseDto
                    {
                        Success = false,
                        Error = errorMessage,
                    };
                }

                return new SaveFlowResponseDto
                {
                    Success = true,
                };
            }
            finally
            {
                compilationFilesCleaner.CleanCompilationFiles(sandboxApplicationDefinition.Name);
            }
        }

        public GetParametersResponseDto GetParameters(GetParametersRequestDto dto)
        {
            var flowContext = FlowContext.Create(dto.FlowData, dto.FlowProperties,
                dbContext, localizationContext, systemTypeContext);
            var action = flowContext.Actions.SingleOrDefault(x => x.Id == dto.ActionId);
            if (action == null)
            {
                return new GetParametersResponseDto
                {
                    ActionId = dto.ActionId,
                    ActionType = dto.ActionType,
                    UpdatedParameters = Enumerable.Empty<ActionParameter>()
                };
            }

            action.GenerateParameters();
            action.Parameters.ForEach(param => FormatParameterDisplayName(param));

            var formulaHints = action.GetFormulaHints();

            return new GetParametersResponseDto
            {
                ActionId = dto.ActionId,
                ActionType = dto.ActionType,
                UpdatedParameters = action.Parameters,
                FormulaHints = formulaHints
            };
        }

        private void FormatParameterDisplayName(ActionParameter param)
        {
            string name;
            if (param.DisableTranslating)
            {
                var dividerIndex = param.Name.LastIndexOf('_');
                var prefix = param.Name.Substring(0, dividerIndex + 1);
                var prefixLocalized = localizationContext.TryTranslate(prefix);
                var propName = param.Name.Substring(dividerIndex + 1);
                name = $"{propName} - {prefixLocalized}";
            }
            else
            {
                name = localizationContext.TryTranslate(param.Name);
            }

            string typeName;
            if (SystemBasicTypes.IsSimpleType(param.Type))
            {
                typeName = localizationContext.TryTranslate($"FlowDesigner_SimpleType_{param.Type}");
            }
            else if (SystemBasicTypes.IsList(param.Type))
            {
                var genericTypeName = SystemBasicTypes.IsSimpleType(param.GenericType)
                    ? localizationContext.TryTranslate($"FlowDesigner_SimpleType_{param.GenericType}")
                    : localizationContext.TryTranslate(param.GenericType);
                typeName = localizationContext.TryTranslate("FlowDesigner_SimpleType_List") + $" ({genericTypeName})";
            }
            else
            {
                typeName = localizationContext.TryTranslate("FlowDesigner_ComplexType_Struct") + $" ({param.Type})";
            }
            var type = string.IsNullOrEmpty(param.Type) ? "" : $", {typeName}";

            var required = localizationContext.TryTranslate($"FlowDesigner_IsRequired_{param.IsRequired}");

            param.DisplayName = name;
            param.DisplayDescription = $"{required}{type}";
        }

        public CreateNewActionResponseDto CreateNewAction(CreateNewActionRequestDto dto)
        {
            var newAction = new NodeData
            {
                Id = Guid.NewGuid().ToString(),
                Position = new NodePosition(),
                Data = new NodeDataData
                {
                    Label = localizationContext.TryTranslate("FlowDesigner_NewActionName"),
                    Type = "Return",
                    Parameters = new []
                    {
                        new ActionParameter 
                        { 
                            Name = "Return_Value",
                            Type = $"{dto.FlowName}ResponseDto",
                            IsRequired = true,
                            DisplayName = localizationContext.TryTranslate("Return_Value"),
                            DisplayDescription = localizationContext.TryTranslate($"FlowDesigner_IsRequired_True") + ", " +
                                localizationContext.TryTranslate("FlowDesigner_ComplexType_Struct") + $" ({dto.FlowName}ResponseDto)"
                        },
                    }
                }
            };

            return new CreateNewActionResponseDto
            {
                NewNode = newAction
            };
        }

        public ModifyDtosDueToTypeChangeResponseDto ModifyDtosDueToTypeChange(ModifyDtosDueToTypeChangeRequestDto dto)
        {
            var modifiedDtos = new List<TypeData>();

            var requestDtoName = $"{dto.FlowProperties.FlowName}RequestDto";
            var responseDtoName = $"{dto.FlowProperties.FlowName}ResponseDto";

            var withoutRequestAndResponseDtos = dto.TypesData
                .Where(type => type.Name != requestDtoName && type.Name != responseDtoName);
            modifiedDtos.AddRange(withoutRequestAndResponseDtos);

            if (dto.NewType == FlowTypes.GetResource)
            {
                var newResponseDto = new TypeData { Name = responseDtoName, PreventDelete = true, Properties = Enumerable.Empty<TypeProperty>() };
                modifiedDtos.Add(newResponseDto);
            }
            else if (dto.NewType == FlowTypes.ModifyResource)
            {
                var newRequestDto = new TypeData { Name = requestDtoName, PreventDelete = true, Properties = Enumerable.Empty<TypeProperty>() };
                modifiedDtos.Add(newRequestDto);

                var newResponseDto = new TypeData { Name = responseDtoName, PreventDelete = true, Properties = Enumerable.Empty<TypeProperty>() };
                modifiedDtos.Add(newResponseDto);
            }
            else if (dto.NewType == FlowTypes.DownloadFile)
            {
                var newRequestDto = new TypeData { Name = requestDtoName, PreventDelete = true, Properties = Enumerable.Empty<TypeProperty>() };
                modifiedDtos.Add(newRequestDto);

                var newResponseDto = new TypeData
                {
                    Name = responseDtoName,
                    PreventDelete = true,
                    PreventEdit = true,
                    Properties = new List<TypeProperty>
                    {
                        new TypeProperty { Name = "FileName", Type = SystemBasicTypes.TextPhrase },
                        new TypeProperty { Name = "FilePath", Type = SystemBasicTypes.TextPhrase },
                        new TypeProperty { Name = "FileType", Type = SystemBasicTypes.TextPhrase }
                    }
                };
                modifiedDtos.Add(newResponseDto);
            }
            else if (dto.NewType == FlowTypes.UploadFile)
            {
                var newRequestDto = new TypeData 
                { 
                    Name = requestDtoName, 
                    PreventDelete = true,
                    Properties = new List<TypeProperty>
                    {
                        new TypeProperty { Name = "File", Type = SystemBasicTypes.File }
                    }
                };
                modifiedDtos.Add(newRequestDto);

                var newResponseDto = new TypeData { Name = responseDtoName, PreventDelete = true, Properties = Enumerable.Empty<TypeProperty>() };
                modifiedDtos.Add(newResponseDto);
            }
            else
            {
                throw new NotSupportedException($"Cannot modify DTOs of the flow - the flow type {dto.NewType} does not exist.");
            }

            return new ModifyDtosDueToTypeChangeResponseDto
            {
                TypesData = modifiedDtos
            };
        }
    }
}
