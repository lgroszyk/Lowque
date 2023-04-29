using Lowque.BusinessLogic.Dto.ApplicationDefinition;
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
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Lowque.BusinessLogic.Services
{
    public class ApplicationDefinitionService : IApplicationDefinitionService
    {
        private readonly AppDbContext dbContext;
        private readonly ILocalizationContext localizationContext;
        private readonly ISolutionGenerator solutionGenerator;
        private readonly ISolutionCompilator solutionCompilator;
        private readonly ICompilationErrorsFormatter compilationErrorsFormatter;
        private readonly ICompilationFilesCleaner compilationFilesCleaner;
        private readonly ITypeSpecificationFormatter typeSpecificationFormatter;
        private readonly IWebHostEnvironment webHostEnvironment;

        public ApplicationDefinitionService(AppDbContext dbContext, 
            ILocalizationContext localizationContext, 
            ISolutionGenerator solutionGenerator,
            ISolutionCompilator solutionCompilator,           
            ICompilationErrorsFormatter compilationErrorsFormatter,
            ICompilationFilesCleaner compilationFilesCleaner,
            ITypeSpecificationFormatter typeSpecificationFormatter,
            IWebHostEnvironment webHostEnvironment)
        {
            this.dbContext = dbContext;
            this.localizationContext = localizationContext;
            this.solutionGenerator = solutionGenerator;
            this.solutionCompilator = solutionCompilator;
            this.compilationErrorsFormatter = compilationErrorsFormatter;
            this.compilationFilesCleaner = compilationFilesCleaner;
            this.typeSpecificationFormatter = typeSpecificationFormatter;
            this.webHostEnvironment = webHostEnvironment;
        }

        public GetExistingApplicationsAndTemplatesResponseDto GetExistingApplicationsAndTemplates()
        {
            var existingApplications = dbContext.ApplicationDefinitions
                .Where(app => !app.IsTemplate)
                .Select(app => new BasicAppInfo 
                { 
                    Id = app.ApplicationDefinitionId,
                    Name = app.Name, 
                    CreatedAt = app.CreatedAt.ToString("yyyy-MM-dd") 
                });
            var existingTemplates = dbContext.ApplicationDefinitions
                .Where(app => app.IsTemplate)
                .Select(app => app.Name);

            return new GetExistingApplicationsAndTemplatesResponseDto
            {
                Applications = existingApplications,
                Templates = existingTemplates
            };
        }

        public GetFlowDefinitionsResponseDto GetFlowDefinitions(int id)
        {
            var flowDefinitions = dbContext.FlowDefinitions
                .Where(flow => flow.ApplicationDefinitionId == id)
                .OrderBy(flow => flow.Area)
                .ThenBy(flow => flow.Name);

            var flowsData = flowDefinitions.Select(flow => new BasicFlowInfo
                {
                    Id = flow.FlowDefinitionId,
                    Name = flow.Name,
                    Area = flow.Area
                });
            var specData = GetApplicationSpecification(flowDefinitions);

            return new GetFlowDefinitionsResponseDto
            {
                Flows = flowsData,
                Spec = specData
            };
        }

        public GetApplicationDefinitionResponseDto GetApplicationDefinition(int id)
        {
            var appDefinition = dbContext.ApplicationDefinitions
                .Single(app => app.ApplicationDefinitionId == id);
            return new GetApplicationDefinitionResponseDto
            {
                Id = appDefinition.ApplicationDefinitionId,
                Name = appDefinition.Name,
                CreatedAt = appDefinition.CreatedAt.ToString("yyyy-MM-dd"),
            };
        }

        public CreateApplicationDefinitionResponseDto CreateApplicationDefinition(CreateApplicationDefinitionRequestDto dto)
        {
            var isFirstUsableApplication = !dbContext.ApplicationDefinitions.Any(app => !app.IsTemplate);
            if (!isFirstUsableApplication)
            {
                return new CreateApplicationDefinitionResponseDto
                {
                    Success = false,
                    Error = localizationContext.TryTranslate("AppDefinitions_Error_OtherApplicationExist")
                };
            }

            var applicationTemplate = dbContext.ApplicationDefinitions
                .Include(app => app.BusinessLogicModuleDefinition)
                .Single(x => x.IsTemplate && x.Name == dto.Template);

            var createdApp = dbContext.ApplicationDefinitions.Add(new ApplicationDefinition
            {
                Name = applicationTemplate.Name.Split('_').First(),
                Template = applicationTemplate.Name,
                IsTemplate = false,
                CreatedAt = DateTime.Now,
                IdentityModuleDefinition = applicationTemplate.IdentityModuleDefinition,
                DataAccessModuleDefinition = applicationTemplate.DataAccessModuleDefinition,
                BusinessLogicModuleDefinition = applicationTemplate.BusinessLogicModuleDefinition?.Select(flow => new FlowDefinition 
                { 
                    Name = flow.Name, 
                    Area = flow.Area, 
                    Type = flow.Type, 
                    UseResourceIdentifier = flow.UseResourceIdentifier, 
                    Content = flow.Content 
                }).ToList(),
            });
            dbContext.SaveChanges();

            return new CreateApplicationDefinitionResponseDto
            {
                Success = true,
                ApplicationId = createdApp.Entity.ApplicationDefinitionId
            };
        }

        public CreateFlowDefinitionResponseDto CreateFlowDefinition(CreateFlowDefinitionRequestDto dto)
        {
            var isFlowNameDuplicate = dbContext.FlowDefinitions.Any(flow => 
                flow.Name == dto.Name && 
                flow.Area == dto.Area && 
                flow.ApplicationDefinitionId == dto.ApplicationId);
            if (isFlowNameDuplicate)
            {
                return new CreateFlowDefinitionResponseDto
                {
                    Success = false,
                    Error = localizationContext.TryTranslate("AppDefinition_Error_FlowNameNotUnique")
                };
            }

            var newFlow = new FlowDefinition
            {
                Name = dto.Name,
                Area = dto.Area,
                Type = FlowTypes.GetResource,
                Content = $"{{\"NodesData\":[],\"EdgesData\":[],\"TypesData\":[{{\"Name\":\"{dto.Name}ResponseDto\",\"PreventDelete\":true,\"Properties\":[]}}]}}",
                ApplicationDefinitionId = dto.ApplicationId
            };
            dbContext.FlowDefinitions.Add(newFlow);
            dbContext.SaveChanges();

            return new CreateFlowDefinitionResponseDto
            {
                Success = true,
                FlowId = newFlow.FlowDefinitionId
            };
        }

        public DeleteFlowDefinitionResponseDto DeleteFlowDefinition(int id)
        {
            var flowToDelete = dbContext.FlowDefinitions
                .SingleOrDefault(flow => flow.FlowDefinitionId == id);
            if (flowToDelete == null)
            {
                return new DeleteFlowDefinitionResponseDto
                {
                    Success = false,
                    Error = localizationContext.TryTranslate("AppDefinition_Error_FlowDoesNotExists")
                };
            }

            dbContext.FlowDefinitions.Remove(flowToDelete);
            dbContext.SaveChanges();

            return new DeleteFlowDefinitionResponseDto
            {
                Success = true
            };
        }

        public DeployApplicationResponseDto DeployApplication(DeployApplicationRequestDto dto)
        {
            var applicationDefinition = dbContext.ApplicationDefinitions
                .Include(app => app.BusinessLogicModuleDefinition)
                .Single(app => app.ApplicationDefinitionId == dto.AppId);

            try
            {
                var filesGenerationResult = solutionGenerator.Generate(applicationDefinition, new SolutionOptions(SolutionType.Application));
                if (filesGenerationResult.FlowGenerationErrors.Any())
                {
                    var invalidFlowsNames = filesGenerationResult.FlowGenerationErrors.Select(error => error.FlowName);
                    return new DeployApplicationResponseDto
                    {
                        Success = false,
                        Error = localizationContext.TryTranslate("AppDefinition_Error_Generation_DeployFailure", 
                            string.Join(", ", invalidFlowsNames))
                    };
                }

                var compilationResult = solutionCompilator.Compile(applicationDefinition.Name, new SolutionOptions(SolutionType.Application));
                var compilationResultErrors = compilationErrorsFormatter.Format(filesGenerationResult, compilationResult);
                if (compilationResultErrors.Any())
                {
                    var invalidFlowsNames = compilationResultErrors.Select(error => error.FlowName);
                    return new DeployApplicationResponseDto
                    {
                        Success = false,
                        Error = localizationContext.TryTranslate("AppDefinition_Error_Compilation_DeployFailure", 
                            string.Join(", ", invalidFlowsNames))
                    };
                }

                return new DeployApplicationResponseDto
                {
                    Success = true,
                };
            }
            finally
            {
                compilationFilesCleaner.CleanCompilationFiles(applicationDefinition.Name);
            }
        }

        public DownloadSpecificationResponseDto DownloadSpecification(int id)
        {
            var flowDefinitions = dbContext.FlowDefinitions
                .Where(flow => flow.ApplicationDefinitionId == id)
                .OrderBy(flow => flow.Area)
                .ThenBy(flow => flow.Name);
            var specData = GetApplicationSpecification(flowDefinitions);

            var specHtml = new StringBuilder();

            specHtml.AppendLine($"<div><h2>{localizationContext.TryTranslate("FlowDefinitions_SpecModal_Header")}</h2></div>");
            specHtml.AppendLine("<br/>");

            foreach (var areaData in specData)
            {
                specHtml.AppendLine("<div>");
                specHtml.AppendLine($"<div><h3>{areaData.Key}</h3></div>");
                foreach (var flowData in areaData.Value)
                {
                    specHtml.AppendLine("<div class='spec-flow-wrapper'>");
                    specHtml.AppendLine($"<h4>{flowData.FlowName}</h4>");

                    specHtml.AppendLine($"<div>{localizationContext.TryTranslate("FlowDefinitions_SpecModal_FlowHttpMethod")}: <strong>{flowData.HttpMethod}</strong></div>");
                    specHtml.AppendLine($"<div>{localizationContext.TryTranslate("FlowDefinitions_SpecModal_RequestUrl")}: <strong>{flowData.RequestUrl}</strong></div>");
                    specHtml.AppendLine($"<div>{localizationContext.TryTranslate("FlowDefinitions_SpecModal_RequestBodyType")}: <strong>{flowData.RequestBodyType}</strong></div>");
                    if (!string.IsNullOrEmpty(flowData.RequestBodySchema))
                    {
                        specHtml.AppendLine($"<div>{localizationContext.TryTranslate("FlowDefinitions_SpecModal_RequestBodySchema")}: <strong>{flowData.RequestBodySchema}</strong></div>");
                    }
                    specHtml.AppendLine($"<div>{localizationContext.TryTranslate("FlowDefinitions_SpecModal_ResponseBodySchema")}: <strong>{flowData.ResponseBodySchema}</strong></div>");
                    specHtml.AppendLine("<br/>");

                    specHtml.AppendLine($"<div>{localizationContext.TryTranslate("FlowDefinitions_SpecModal_Schemas")}:</div>");
                    specHtml.AppendLine("<br/>");

                    foreach (var type in flowData.Types)
                    {
                        specHtml.AppendLine("<div>");
                        specHtml.AppendLine($"<div>{type.TypeName}</div>");
                        specHtml.AppendLine($"<div>{{</div>");
                        specHtml.AppendLine("<div class='spec-flow-type-props-wrapper'>");
                        foreach (var prop in type.TypeProperties)
                        {
                            specHtml.AppendLine($"<div>{HttpUtility.HtmlEncode(prop)}</div>");
                        }
                        specHtml.AppendLine("</div>");
                        specHtml.AppendLine($"<div>}}</div>");
                        specHtml.AppendLine("</div>");
                        specHtml.AppendLine("<br/>");
                    }

                    specHtml.AppendLine("</div>");
                }
                specHtml.AppendLine("</div>");
            }
            
            var templatePath = Path.Combine(webHostEnvironment.ContentRootPath, "ClientApp", "private", "specification_template.html");
            var templateContent = File.ReadAllText(templatePath);
            var specificationPath = Path.Combine(webHostEnvironment.ContentRootPath, "ClientApp", "private", "specification.html");
            var specificationContent = templateContent.Replace("<div>content</div>", specHtml.ToString());
            File.WriteAllText(specificationPath, specificationContent);

            return new DownloadSpecificationResponseDto
            {
                FileName = "specification.html",
                FilePath = specificationPath,
                FileType = "text/html",
            };
        }

        private IDictionary<string, List<FlowSpecification>> GetApplicationSpecification(IEnumerable<FlowDefinition> flowDefinitions)
        {
            var specData = new Dictionary<string, List<FlowSpecification>>();
            foreach (var flowDefinition in flowDefinitions)
            {
                var flowSpec = new FlowSpecification
                {
                    FlowName = flowDefinition.Name,
                    HttpMethod = flowDefinition.Type == FlowTypes.GetResource ? "GET" : "POST",
                    RequestUrl = $"{flowDefinition.Area}/{flowDefinition.Name}" + (flowDefinition.UseResourceIdentifier ? "/{id}" : ""),
                    RequestBodyType = flowDefinition.Type == FlowTypes.UploadFile ? "application/x-www-url-formencoded" : "application/json",
                    RequestBodySchema = flowDefinition.Type == FlowTypes.GetResource ? null : $"{flowDefinition.Name}RequestDto",
                    ResponseBodySchema = $"{flowDefinition.Name}ResponseDto",
                    Types = new List<FlowSpecificationType>()
                };
                var flowTypes = JsonConvert.DeserializeObject<FlowContent>(flowDefinition.Content).TypesData;
                foreach (var flowType in flowTypes)
                {
                    var typeFormatted = typeSpecificationFormatter.Format(flowType);
                    flowSpec.Types.Add(typeFormatted);
                }

                if (!specData.ContainsKey(flowDefinition.Area))
                {
                    specData.Add(flowDefinition.Area, new List<FlowSpecification>());
                }
                specData[flowDefinition.Area].Add(flowSpec);
            }
            return specData;
        }
    }
}
