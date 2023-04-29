using Lowque.BusinessLogic.SolutionGeneration;
using Lowque.BusinessLogic.Types;
using Lowque.DataAccess.Internationalization.Interfaces;
using Lowque.DataAccess.SolutionCompilation;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lowque.BusinessLogic.SolutionCompilation
{
    public class CompilationErrorsFormatter : ICompilationErrorsFormatter
    {
        private readonly ILocalizationContext localizationContext;
        private readonly IDictionary<int, string> customErrorMessageTemplates;

        public CompilationErrorsFormatter(ILocalizationContext localizationContext)
        {
            this.localizationContext = localizationContext;
            customErrorMessageTemplates = GetCustomErrorMessageTemplates();
        }

        public IEnumerable<CompilationError> Format(GenerationResult generationResult, CompilationResult compilationResult)
        {
            var compilationErrors = new List<CompilationError>();
            foreach (var errorLog in compilationResult.Logs)
            {
                var classNameRegex = @"\\([A-Za-z]+)Service\.cs";
                var classNameMatch = Regex.Match(errorLog, classNameRegex);
                var flowArea = classNameMatch.Groups[1].Value;

                var lineNumberRegex = @"Service\.cs\((\d+),";
                var lineNumberMatch = Regex.Match(errorLog, lineNumberRegex);
                var lineNumber = int.Parse(lineNumberMatch.Groups[1].Value);

                var errorCodeRegex = @"error CS(\d{4}):";
                var errorCodeMatch = Regex.Match(errorLog, errorCodeRegex);
                if (errorCodeMatch.Groups.Count < 2)
                {
                    continue; 
                }
                var errorCode = int.Parse(errorCodeMatch.Groups[1].Value);

                var errorMessageRegex = @"error CS\d{4}: (.*)\[";
                var errorMessageMatch = Regex.Match(errorLog, errorMessageRegex);
                var cSharpCompilerErrorMessage = errorMessageMatch.Groups[1].Value;

                var errorMessage = cSharpCompilerErrorMessage;
                var customMessageForThisErrorExists = customErrorMessageTemplates.TryGetValue(errorCode, out string customErrorMessage);
                if (customMessageForThisErrorExists)
                {
                    var errorMessageParameters = MatchPhrasesEnclosedInQuotationMarks(cSharpCompilerErrorMessage)
                        .Select(phrase => GetLastPartOfTypeName(phrase))
                        .Select(phrase => GetCorrespondingSystemTypeName(phrase));
                    errorMessage = localizationContext.TryTranslate(customErrorMessage, errorMessageParameters);
                }

                var lineInfo = generationResult.FlowGenerationMaps.Single(map => map.FlowArea == flowArea).LineActionMap[lineNumber];
                compilationErrors.Add(new CompilationError
                {
                    FlowArea = flowArea,
                    ActionName = lineInfo.ActionName,
                    FlowName = lineInfo.FlowName,
                    Error = errorMessage
                });
            }
            return compilationErrors;
        }

        private IEnumerable<string> MatchPhrasesEnclosedInQuotationMarks(string fullPhrase)
        {
            var enclosedPhrases = new List<string>();
            var includeInCurrentPhrase = false;
            var currentPhrase = string.Empty;
            var quotationMarkChars = new[] { '"', '\'', '„', '”' };
            for (int i = 0; i < fullPhrase.Length; i++)
            {
                var currentChar = fullPhrase[i];

                var switchedIncludeFlag = false;
                if (quotationMarkChars.Any(quotationMarkChar => quotationMarkChar == currentChar))
                {
                    switchedIncludeFlag = true;
                    includeInCurrentPhrase = !includeInCurrentPhrase;
                    if (includeInCurrentPhrase)
                    {
                        continue;
                    }
                }

                if (includeInCurrentPhrase)
                {
                    currentPhrase += currentChar;
                }

                if (!includeInCurrentPhrase && switchedIncludeFlag)
                {
                    enclosedPhrases.Add(currentPhrase);
                    currentPhrase = string.Empty;
                }
            }
            return enclosedPhrases;
        }

        private string GetLastPartOfTypeName(string typeName)
        {
            return typeName.Split('.').Last();
        }

        private string GetCorrespondingSystemTypeName(string typeName)
        {
            return SystemBasicTypes.AsSystemType(typeName);
        }

        private IDictionary<int, string> GetCustomErrorMessageTemplates()
        {
            return new Dictionary<int, string>
            {
                { 19, "FlowDesigner_CompilationError_OperandCannotBeApplied_TwoOperands" },
                { 21, "FlowDesigner_CompilationError_CannotApplyIndexing" },
                { 23, "FlowDesigner_CompilationError_OperandCannotBeApplied_OneOperand" },
                { 29, "FlowDesigner_CompilationError_CannotConvert" },
                { 30, "FlowDesigner_CompilationError_CannotConvert" },
                { 31, "FlowDesigner_CompilationError_ConstantCannotBeConvertedTo" },
                { 103, "FlowDesigner_CompilationError_NameDoesNotExistInCurrentContext" },
                { 117, "FlowDesigner_CompilationError_DoesNotContainDefinition" },
                { 128, "FlowDesigner_CompilationError_VariableAlreadyDefined" },
                { 161, "FlowDesigner_CompilationError_NotAllPathsReturnValue" },
                { 165, "FlowDesigner_CompilationError_UnassignedVariable" },
                { 234, "FlowDesigner_CompilationError_TypeNameNotExist" },
                { 246, "FlowDesigner_CompilationError_TypeNameNotExist" },
                { 266, "FlowDesigner_CompilationError_CannotConvert" },
                { 844, "FlowDesigner_CompilationError_VariableNotExist" },
                { 1026, "FlowDesigner_CompilationError_RightBracketExpected" },
                { 1039, "FlowDesigner_CompilationError_UnterminatedStringLiteral" },
                { 1501, "FlowDesigner_CompilationError_BadNumberOfArguments" },
                { 1503, "FlowDesigner_CompilationError_CannotConvertArgument" },
                { 1513, "FlowDesigner_CompilationError_RightCurlyBracketExpected" },
                { 1514, "FlowDesigner_CompilationError_LeftCurlyBracketExpected" },
                { 7036, "FlowDesigner_CompilationError_NoArgumentGiven" }
            };
        }
    }
}
