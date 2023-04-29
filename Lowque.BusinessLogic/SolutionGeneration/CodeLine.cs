namespace Lowque.BusinessLogic.SolutionGeneration
{
    public class CodeLine
    {
        public string Content { get; set; }
        public CodeIndent Indent { get; set; }
        public string ActionName { get; set; }
        public string FlowName { get; set; }

        public CodeLine(string content, CodeIndent indent)
        {
            Content = content;
            Indent = indent;
        }

        public CodeLine(string content, CodeIndent indent, string flowName, string actionName)
        {
            Content = content;
            Indent = indent;
            FlowName = flowName;
            ActionName = actionName;
        }
    }
}
