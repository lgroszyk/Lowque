using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lowque.BusinessLogic.SolutionGeneration
{
    public class CodeGenerator
    {
        public static CodeGenerationResult GenerateCode(IEnumerable<CodeLine> codeLinesInfo)
        {
            var builder = new StringBuilder();
            var map = new CodeLineFlowActionMap();

            var indentsCount = 0;
            for (var lineNumber = 1; lineNumber <= codeLinesInfo.Count(); lineNumber++)
            {
                var codeLineInfo = codeLinesInfo.ElementAt(lineNumber - 1);

                if (codeLineInfo.Indent == CodeIndent.DeleteBefore)
                {
                    indentsCount--;
                }

                builder.AppendLine($"{GenerateIndent(indentsCount)}{codeLineInfo.Content}");
                if (codeLineInfo.FlowName != null)
                {
                    map.Add(lineNumber, new FlowNameActionNamePair
                    { 
                        ActionName = codeLineInfo.ActionName,
                        FlowName = codeLineInfo.FlowName
                    });
                }

                if (codeLineInfo.Indent == CodeIndent.AddAfter)
                {
                    indentsCount++;
                }
            }

            return new CodeGenerationResult
            {
                GeneratedCode = builder.ToString(),
                CodeLineFlowActionMap = map
            };
        }

        private static string GenerateIndent(int indentsCount)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < indentsCount; i++)
            {
                builder.Append('\t');
            }
            return builder.ToString();
        }
    }
}
