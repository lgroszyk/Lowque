using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Lowque.BusinessLogic.FlowStructure;
using Lowque.BusinessLogic.FlowStructure.FlowComponents;
using Lowque.BusinessLogic.Types;
using Lowque.DataAccess;
using Lowque.DataAccess.Entities;
using Lowque.DataAccess.Internationalization.Interfaces;
using Lowque.DataAccess.SolutionCompilation;
using Lowque.DataAccess.Utils;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Lowque.BusinessLogic.SolutionGeneration
{
    public class SolutionGenerator : ISolutionGenerator
    {
        private ApplicationDefinition appDefinition;
        private SolutionOptions options;
        private readonly AppDbContext dbContext;
        private readonly ISystemTypesContext systemTypesContext;
        private readonly ILocalizationContext localizationContext;
        private readonly IConfiguration appConfiguration;

        public SolutionGenerator(AppDbContext dbContext, 
            ISystemTypesContext systemTypesContext,
            ILocalizationContext localizationContext,
            IConfiguration appConfiguration)
        {
            this.dbContext = dbContext;
            this.systemTypesContext = systemTypesContext;
            this.localizationContext = localizationContext;
            this.appConfiguration = appConfiguration;
            this.options = SolutionOptions.Default;
        }

        public GenerationResult Generate(ApplicationDefinition appDefinition)
        {
            this.appDefinition = appDefinition;

            var templateGenerationResult = GenerateAppTemplateFiles();
            if (!templateGenerationResult.Success)
            {
                return templateGenerationResult;
            }

            var flowsGenerationResult = GenerateBusinessLogicModuleFiles();
            return flowsGenerationResult;
        }

        public GenerationResult Generate(ApplicationDefinition appDefinition, SolutionOptions options)
        {
            this.options = options;
            return Generate(appDefinition);
        }

        private GenerationResult GenerateAppTemplateFiles()
        {
            var workspace = appConfiguration["Configuration:Workspace"];
            var sourcePath = Path.Combine(workspace, "Templates", appDefinition.Template);

            if (!Directory.Exists(sourcePath))
            {
                return new GenerationResult
                {
                    Success = false,
                    TemplateGenerationErrors = new[] { new TemplateError 
                        { ErrorMessage = localizationContext.TryTranslate("AppDefinition_Error_TemplateNotExist") } }
                };
            }

            var targetPath = Path.Combine(GetTargetPathRoot(), appDefinition.Name);
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }
            IoExtensions.ClearDirectory(targetPath);

            var slnFileName = $"{appDefinition.Name}.sln";
            File.Copy(Path.Combine(sourcePath, slnFileName), Path.Combine(targetPath, slnFileName));

            var uilDirectoryName = $"{appDefinition.Name}.WebApp";
            IoExtensions.CopyDirectory(Path.Combine(sourcePath, uilDirectoryName), Path.Combine(targetPath, uilDirectoryName), GetIgnoredItems());
            File.Copy(Path.Combine(sourcePath, "BuildDependentFiles", GetProjectFileName()),
                Path.Combine(targetPath, uilDirectoryName, $"{appDefinition.Name}.WebApp.csproj"));

            var bllDirectoryName = $"{appDefinition.Name}.BusinessLogic";
            IoExtensions.CopyDirectory(Path.Combine(sourcePath, bllDirectoryName), Path.Combine(targetPath, bllDirectoryName));

            var dalDirectoryName = $"{appDefinition.Name}.DataAccess";
            IoExtensions.CopyDirectory(Path.Combine(sourcePath, dalDirectoryName), Path.Combine(targetPath, dalDirectoryName));

            return new GenerationResult
            {
                Success = true,
            };
        }

        private GenerationResult GenerateBusinessLogicModuleFiles()
        {
            List<FlowAreaFlowActionCodeMap> lineActionMaps = new List<FlowAreaFlowActionCodeMap>();

            var targetContainer = Path.Combine(GetTargetPathRoot(), appDefinition.Name);
            var businessLogicRoot = Path.Combine(targetContainer, $"{appDefinition.Name}.BusinessLogic");
            var presentationRoot = Path.Combine(targetContainer, $"{appDefinition.Name}.WebApp");

            var flowsGroupsByArea = appDefinition.BusinessLogicModuleDefinition.GroupBy(flow => flow.Area);
            foreach (var flowsGroup in flowsGroupsByArea)
            {
                var area = flowsGroup.Key;
                foreach (var flowDefinition in flowsGroup)
                {
                    var dtosDirPath = Path.Combine(businessLogicRoot, "Dto", area);
                    Directory.CreateDirectory(dtosDirPath);

                    var flow = JsonConvert.DeserializeObject<FlowContent>(flowDefinition.Content);
                    var dtosCodeInfo = GenerateFlowDtos(flow, area);
                    var dtosCode = CodeGenerator.GenerateCode(dtosCodeInfo);

                    var dtosFilePath = Path.Combine(dtosDirPath, $"{flowDefinition.Name}.cs");
                    File.WriteAllText(dtosFilePath, dtosCode.GeneratedCode);
                }

                var controllerCodeInfo = GenerateController(area, flowsGroup);
                var controllerCode = CodeGenerator.GenerateCode(controllerCodeInfo);

                var controllerFilePath = Path.Combine(presentationRoot, "Controllers", $"{area}Controller.cs");
                File.WriteAllText(controllerFilePath, controllerCode.GeneratedCode);

                var serviceInterfaceCodeInfo = GenerateServiceInterface(area, flowsGroup);
                var serviceInterfaceCode = CodeGenerator.GenerateCode(serviceInterfaceCodeInfo);
                var serviceInterfaceFilePath = Path.Combine(businessLogicRoot, "Services", "Interfaces", $"I{area}Service.cs");
                File.WriteAllText(serviceInterfaceFilePath, serviceInterfaceCode.GeneratedCode);

                var serviceImplementationCodeInfo = GenerateServiceImplementation(area, flowsGroup);
                if (!serviceImplementationCodeInfo.Success)
                {
                    return new GenerationResult
                    {
                        Success = false,
                        FlowGenerationErrors = serviceImplementationCodeInfo.Errors
                    };
                }

                var serviceImplementationCode = CodeGenerator.GenerateCode(serviceImplementationCodeInfo.Code);
                var serviceImplementationFilePath = Path.Combine(businessLogicRoot, "Services", $"{area}Service.cs");
                File.WriteAllText(serviceImplementationFilePath, serviceImplementationCode.GeneratedCode);
                lineActionMaps.Add(new FlowAreaFlowActionCodeMap { FlowArea = area, LineActionMap = serviceImplementationCode.CodeLineFlowActionMap });
            }

            var flowsAreas = flowsGroupsByArea.Select(group => group.Key);
            var servicesRegistrationCodeInfo = GenerateServicesRegistration(flowsAreas);
            var servicesRegistrationCode = CodeGenerator.GenerateCode(servicesRegistrationCodeInfo);
            var servicesRegistrationFilePath = Path.Combine(presentationRoot, "StartupServices.cs");
            File.WriteAllText(servicesRegistrationFilePath, servicesRegistrationCode.GeneratedCode);

            return new GenerationResult
            { 
                Success = true,
                FlowGenerationErrors = Enumerable.Empty<FlowError>(),
                FlowGenerationMaps = lineActionMaps
            };
        }

        private ServicesImplementationResult GenerateServiceImplementation(string area, IEnumerable<FlowDefinition> flows)
        {
            var code = new List<CodeLine>();

            code.AddRange(new List<CodeLine>
            {
                new CodeLine($"using System;", CodeIndent.DoNotChange),
                new CodeLine($"using System.Collections.Generic;", CodeIndent.DoNotChange),
                new CodeLine($"using System.Linq;", CodeIndent.DoNotChange),
                new CodeLine($"using System.Text;", CodeIndent.DoNotChange),
                new CodeLine($"using System.Threading.Tasks;", CodeIndent.DoNotChange),
                new CodeLine($"using Microsoft.EntityFrameworkCore;", CodeIndent.DoNotChange),
                new CodeLine($"using {appDefinition.Name}.BusinessLogic.Dto.{area};", CodeIndent.DoNotChange),
                new CodeLine($"using {appDefinition.Name}.BusinessLogic.Services.Interfaces;", CodeIndent.DoNotChange),
                new CodeLine($"using {appDefinition.Name}.DataAccess;", CodeIndent.DoNotChange),
                new CodeLine($"using {appDefinition.Name}.DataAccess.Entities;", CodeIndent.DoNotChange),
                new CodeLine($"using {appDefinition.Name}.DataAccess.Entities.Identity;", CodeIndent.DoNotChange),
                new CodeLine($"using {appDefinition.Name}.DataAccess.Internationalization.Interfaces;", CodeIndent.DoNotChange),
                new CodeLine($"using {appDefinition.Name}.DataAccess.Utils.Extensions;", CodeIndent.DoNotChange),
                new CodeLine($"using {appDefinition.Name}.DataAccess.Utils.Interfaces;", CodeIndent.DoNotChange),
            });
            code.Add(new CodeLine("", CodeIndent.DoNotChange));

            code.Add(new CodeLine($"namespace {appDefinition.Name}.BusinessLogic.Services", CodeIndent.DoNotChange));
            code.Add(new CodeLine("{", CodeIndent.AddAfter));

            code.Add(new CodeLine($"public class {area}Service : I{area}Service", CodeIndent.DoNotChange));
            code.Add(new CodeLine("{", CodeIndent.AddAfter));

            var serviceDependencyFields = new List<string>();
            var methodsCode = new List<CodeLine>();
            var flowValidationErrors = new List<FlowError>();
            
            for (var i = 0; i < flows.Count(); i++)
            {
                var flow = flows.ElementAt(i);
                var flowContent = JsonConvert.DeserializeObject<FlowContent>(flow.Content);
                var flowProperties = new FlowProperties
                {
                    AppId = appDefinition.ApplicationDefinitionId, AppName = appDefinition.Name, UseResourceIdentifier = flow.UseResourceIdentifier,
                    FlowId = flow.FlowDefinitionId, FlowName = flow.Name, FlowArea = flow.Area, FlowType = flow.Type,
                };
                var flowContext = FlowContext.Create(flowContent, flowProperties,
                    dbContext, localizationContext, systemTypesContext);

                var flowValidationResult = flowContext.Validate(dbContext, localizationContext);
                if (!flowValidationResult.Success)
                {
                    flowValidationErrors.Add(new FlowError
                    {
                        FlowArea = flowProperties.FlowArea,
                        FlowName = flowProperties.FlowName,
                        ErrorMessage = flowValidationResult.Message
                    });
                    continue;
                }

                var methodDependencyFields = new List<string>();
                foreach (var action in flowContext.Actions)
                {
                    methodDependencyFields.AddRange(action.GetDependencyFields());
                }
                serviceDependencyFields.AddRange(methodDependencyFields);

                var methodReturnType = $"{flow.Name}ResponseDto";
                var methodName = flow.Name;
                var methodParameter = flow.Type == FlowTypes.GetResource
                    ? (flow.UseResourceIdentifier ? "int id" : "")
                    : $"{flow.Name}RequestDto dto";
                methodsCode.Add(new CodeLine($"public {methodReturnType} {methodName}({methodParameter})", CodeIndent.DoNotChange, flow.Name, null));
                methodsCode.Add(new CodeLine("{", CodeIndent.AddAfter, flow.Name, null));

                var firstAction = flowContext.Actions.SingleOrDefault(x => x.PreviousActions.Count() == 0);
                if (firstAction != null)
                {
                    var methodBodyCodeInfo = firstAction.GenerateCode();
                    methodsCode.AddRange(methodBodyCodeInfo);
                }

                methodsCode.Add(new CodeLine("}", CodeIndent.DeleteBefore, flow.Name, null));

                if (i != flows.Count() - 1)
                {
                    methodsCode.Add(new CodeLine("", CodeIndent.DoNotChange));
                }
            }

            if (flowValidationErrors.Any())
            {
                return new ServicesImplementationResult
                {
                    Success = false,
                    Errors = flowValidationErrors
                };
            }

            var classFieldsCode = new List<CodeLine>();
            var constructorBodyCode = new List<CodeLine>();
            var constructorParametersListBuilder = new StringBuilder();

            var classFields = serviceDependencyFields.Distinct();
            for (var i = 0; i < classFields.Count(); i++)
            {
                var classField = classFields.ElementAt(i);
                var fieldVar = classField.Split(' ').ElementAt(1);

                classFieldsCode.Add(new CodeLine($"private readonly {classField};", CodeIndent.DoNotChange));
                constructorBodyCode.Add(new CodeLine($"this.{fieldVar} = {fieldVar};", CodeIndent.DoNotChange));
                constructorParametersListBuilder.Append(classField);

                if (i == classFields.Count() - 1)
                {
                    classFieldsCode.Add(new CodeLine("", CodeIndent.DoNotChange));
                }
                else
                {
                    constructorParametersListBuilder.Append(", ");
                }
            }

            code.AddRange(classFieldsCode);

            code.Add(new CodeLine($"public {area}Service({constructorParametersListBuilder.ToString()})", CodeIndent.DoNotChange));
            code.Add(new CodeLine("{", CodeIndent.AddAfter));
            code.AddRange(constructorBodyCode);
            code.Add(new CodeLine("}", CodeIndent.DeleteBefore));
            code.Add(new CodeLine("", CodeIndent.DoNotChange));

            code.AddRange(methodsCode);

            code.Add(new CodeLine("}", CodeIndent.DeleteBefore));
            code.Add(new CodeLine("}", CodeIndent.DeleteBefore));

            return new ServicesImplementationResult
            {
                Success = true,
                Code = code
            };
        }

        private IEnumerable<CodeLine> GenerateController(string area, IEnumerable<FlowDefinition> flows)
        {
            var code = new List<CodeLine>();

            code.AddRange(new List<CodeLine>
            {
                new CodeLine("using Microsoft.AspNetCore.Authorization;", CodeIndent.DoNotChange),
                new CodeLine("using Microsoft.AspNetCore.Mvc;", CodeIndent.DoNotChange),
                new CodeLine($"using {appDefinition.Name}.BusinessLogic.Dto.{area};", CodeIndent.DoNotChange),
                new CodeLine($"using {appDefinition.Name}.BusinessLogic.Services.Interfaces;", CodeIndent.DoNotChange),
            });
            code.Add(new CodeLine("", CodeIndent.DoNotChange));

            code.Add(new CodeLine($"namespace {appDefinition.Name}.WebApp.Controllers", CodeIndent.DoNotChange));
            code.Add(new CodeLine("{", CodeIndent.AddAfter));

            code.Add(new CodeLine($"[ApiController, Route(\"[controller]\"), Authorize]", CodeIndent.DoNotChange));
            code.Add(new CodeLine($"public class {area}Controller : ControllerBase", CodeIndent.DoNotChange));
            code.Add(new CodeLine("{", CodeIndent.AddAfter));

            var areaCamelCase = char.ToLower(area[0]) + area.Substring(1);
            code.Add(new CodeLine($"private readonly I{area}Service {areaCamelCase}Service;", CodeIndent.DoNotChange));
            code.Add(new CodeLine("", CodeIndent.DoNotChange));

            code.Add(new CodeLine($"public {area}Controller(I{area}Service {areaCamelCase}Service)", CodeIndent.DoNotChange));
            code.Add(new CodeLine("{", CodeIndent.AddAfter));
            code.Add(new CodeLine($"this.{areaCamelCase}Service = {areaCamelCase}Service;", CodeIndent.DoNotChange));
            code.Add(new CodeLine("}", CodeIndent.DeleteBefore));
            code.Add(new CodeLine("", CodeIndent.DoNotChange));

            for (var i = 0; i < flows.Count(); i++)
            {
                var flow = flows.ElementAt(i);

                var methodAttribute = flow.Type == FlowTypes.GetResource
                    ? ( flow.UseResourceIdentifier ? $"[HttpGet(\"{flow.Name}/{{id}}\")]" : $"[HttpGet(\"{flow.Name}\")]" )
                    : $"[HttpPost(\"{flow.Name}\")]";
                var methodReturnType = flow.Type == FlowTypes.DownloadFile
                    ? "PhysicalFileResult"
                    : $"{flow.Name}ResponseDto";
                var methodName = flow.Name;
                string methodParameter;
                switch (flow.Type)
                {
                    case FlowTypes.GetResource: methodParameter = flow.UseResourceIdentifier ? "int id" : ""; break;
                    case FlowTypes.ModifyResource:
                    case FlowTypes.DownloadFile: methodParameter = $"{flow.Name}RequestDto dto"; break;
                    case FlowTypes.UploadFile: methodParameter = $"[FromForm]{flow.Name}RequestDto dto"; break;
                    default: throw new NotSupportedException($"Flow type: {flow.Type} is not supported.");
                }

                code.Add(new CodeLine($"{methodAttribute}", CodeIndent.DoNotChange));
                code.Add(new CodeLine($"public {methodReturnType} {methodName}({methodParameter})", CodeIndent.DoNotChange));
                code.Add(new CodeLine("{", CodeIndent.AddAfter));

                if (flow.Type == FlowTypes.DownloadFile)
                {
                    code.Add(new CodeLine($"var responseData = {areaCamelCase}Service.{flow.Name}(dto);", CodeIndent.DoNotChange));
                    code.Add(new CodeLine($"return PhysicalFile(responseData.FilePath, responseData.FileType, responseData.FileName);", CodeIndent.DoNotChange));
                }
                else
                {
                    var serviceMethodParameter = flow.Type == FlowTypes.GetResource
                        ? (flow.UseResourceIdentifier ? "id" : "")
                        : "dto";
                    code.Add(new CodeLine($"return {areaCamelCase}Service.{flow.Name}({serviceMethodParameter});", CodeIndent.DoNotChange));
                }

                code.Add(new CodeLine("}", CodeIndent.DeleteBefore));

                if (i != flows.Count() - 1)
                {
                    code.Add(new CodeLine("", CodeIndent.DoNotChange));
                }
            }

            code.Add(new CodeLine("}", CodeIndent.DeleteBefore));

            code.Add(new CodeLine("}", CodeIndent.DeleteBefore));

            return code;
        }

        private IEnumerable<CodeLine> GenerateServiceInterface(string area, IEnumerable<FlowDefinition> flows)
        {
            var code = new List<CodeLine>();

            code.Add(new CodeLine($"using {appDefinition.Name}.BusinessLogic.Dto.{area};", CodeIndent.DoNotChange));
            code.Add(new CodeLine("", CodeIndent.DoNotChange));

            code.Add(new CodeLine($"namespace {appDefinition.Name}.BusinessLogic.Services.Interfaces", CodeIndent.DoNotChange));
            code.Add(new CodeLine("{", CodeIndent.AddAfter));

            code.Add(new CodeLine($"public interface I{area}Service", CodeIndent.DoNotChange));
            code.Add(new CodeLine("{", CodeIndent.AddAfter));

            foreach (var flow in flows)
            {
                var methodReturnType = $"{flow.Name}ResponseDto";
                var methodName = flow.Name;
                var methodParameter = flow.Type == FlowTypes.GetResource
                    ? (flow.UseResourceIdentifier ? "int id" : "")
                    : $"{flow.Name}RequestDto dto";
                code.Add(new CodeLine($"{methodReturnType} {methodName}({methodParameter});", CodeIndent.DoNotChange));
            }

            code.Add(new CodeLine("}", CodeIndent.DeleteBefore));

            code.Add(new CodeLine("}", CodeIndent.DeleteBefore));

            return code;
        }

        private IEnumerable<CodeLine> GenerateServicesRegistration(IEnumerable<string> flowsAreas)
        {
            var code = new List<CodeLine>();

            code.AddRange(new List<CodeLine>
            {
                new CodeLine("using Microsoft.Extensions.DependencyInjection;", CodeIndent.DoNotChange),
                new CodeLine($"using {appDefinition.Name}.BusinessLogic.Services;", CodeIndent.DoNotChange),
                new CodeLine($"using {appDefinition.Name}.BusinessLogic.Services.Interfaces;", CodeIndent.DoNotChange),
            });
            code.Add(new CodeLine("", CodeIndent.DoNotChange));
            
            code.Add(new CodeLine($"namespace {appDefinition.Name}.WebApp", CodeIndent.DoNotChange));
            code.Add(new CodeLine("{", CodeIndent.AddAfter));

            code.Add(new CodeLine($"public static class StartupServices", CodeIndent.DoNotChange));
            code.Add(new CodeLine("{", CodeIndent.AddAfter));

            code.Add(new CodeLine($"public static void ConfigureCustomServices(IServiceCollection services)", CodeIndent.DoNotChange));
            code.Add(new CodeLine("{", CodeIndent.AddAfter));

            code.Add(new CodeLine($"services.AddTransient<IIdentityService, IdentityService>();", CodeIndent.DoNotChange));
            code.Add(new CodeLine($"services.AddTransient<ILocalizationService, LocalizationService>();", CodeIndent.DoNotChange));
            foreach (var flowArea in flowsAreas)
            {
                code.Add(new CodeLine($"services.AddTransient<I{flowArea}Service, {flowArea}Service>();", CodeIndent.DoNotChange));
            }
            code.Add(new CodeLine("}", CodeIndent.DeleteBefore));

            code.Add(new CodeLine("}", CodeIndent.DeleteBefore));

            code.Add(new CodeLine("}", CodeIndent.DeleteBefore));

            return code;
        }

        private IEnumerable<CodeLine> GenerateFlowDtos(FlowContent flow, string area)
        {
            var code = new List<CodeLine>();

            code.AddRange(new List<CodeLine>
            {
                new CodeLine("using System.Collections.Generic;", CodeIndent.DoNotChange),
                new CodeLine("using System.ComponentModel.DataAnnotations;", CodeIndent.DoNotChange),
                new CodeLine("using Microsoft.AspNetCore.Http;", CodeIndent.DoNotChange),
                new CodeLine($"using {appDefinition.Name}.DataAccess.Validation;", CodeIndent.DoNotChange),
            });
            code.Add(new CodeLine("", CodeIndent.DoNotChange));

            code.Add(new CodeLine($"namespace {appDefinition.Name}.BusinessLogic.Dto.{area}", CodeIndent.DoNotChange));
            code.Add(new CodeLine("{", CodeIndent.AddAfter));

            for (var i = 0; i < flow.TypesData.Count(); i++)
            {
                var type = flow.TypesData.ElementAt(i);
                code.AddRange(GenerateFlowDto(type));

                if (i != flow.TypesData.Count() - 1)
                {
                    code.Add(new CodeLine("", CodeIndent.DoNotChange));
                }
            }

            code.Add(new CodeLine("}", CodeIndent.DeleteBefore));

            return code;
        }

        private IEnumerable<CodeLine> GenerateFlowDto(TypeData type)
        {
            var code = new List<CodeLine>();

            code.Add(new CodeLine($"public class {type.Name}", CodeIndent.DoNotChange));
            code.Add(new CodeLine("{", CodeIndent.AddAfter));

            for (var i = 0; i < type.Properties.Count(); i++)
            {
                var prop = type.Properties.ElementAt(i);
                
                var codeAttributes = new List<string>();
                if (prop.Required)
                {
                    codeAttributes.Add("CustomRequired");
                }
                if (prop.MaxLength.HasValue)
                {
                    codeAttributes.Add($"CustomMaxLength({prop.MaxLength.Value})");
                }
                if (codeAttributes.Any())
                {
                    code.Add(new CodeLine($"[{string.Join(", ", codeAttributes)}]", CodeIndent.DoNotChange));
                }

                var codeType = SystemBasicTypes.IsSimpleType(prop.Type) ? SystemBasicTypes.AsCSharpType(prop.Type) : prop.Type;
                var nullableSign = prop.Nullable ? "?" : "";
                var codeProp = prop.List
                    ? new CodeLine($"public List<{codeType}> {prop.Name} {{ get; set; }} = new List<{codeType}>();", CodeIndent.DoNotChange)
                    : new CodeLine($"public {codeType}{nullableSign} {prop.Name} {{ get; set; }}", CodeIndent.DoNotChange);
                code.Add(codeProp);

                if (i != type.Properties.Count() - 1)
                {
                    code.Add(new CodeLine("", CodeIndent.DoNotChange));
                }
            }

            code.Add(new CodeLine("}", CodeIndent.DeleteBefore));

            return code;
        }

        private string GetProjectFileName()
        {
            switch (options.Type)
            {
                case SolutionType.Application: return $"{appDefinition.Name}.WebApp_WithClientApp.csproj";
                case SolutionType.Temporary: return $"{appDefinition.Name}.WebApp_WithoutClientApp.csproj";
                default: throw GetNotSupportedSolutionTypeException(options.Type);
            }
        }

        private IEnumerable<string> GetIgnoredItems()
        {
            switch (options.Type)
            {
                case SolutionType.Application: return new[] { "ProjectFiles" };
                case SolutionType.Temporary: return new[] { "ProjectFiles", "ClientApp" };
                default: throw GetNotSupportedSolutionTypeException(options.Type);
            }
        }

        private string GetTargetPathRoot()
        {
            var workspace = appConfiguration["Configuration:Workspace"];
            switch (options.Type)
            {
                case SolutionType.Application: return Path.Combine(workspace, "Apps");
                case SolutionType.Temporary: return Path.Combine(workspace, "Temporary");
                default: throw GetNotSupportedSolutionTypeException(options.Type);
            }
        }

        private Exception GetNotSupportedSolutionTypeException(SolutionType type)
        {
            return new NotSupportedException($"The {type} type of SolutionGenerator is not supported.");
        }
    }
}
    