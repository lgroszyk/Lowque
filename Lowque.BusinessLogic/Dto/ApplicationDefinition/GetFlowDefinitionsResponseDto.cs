using System.Collections.Generic;

namespace Lowque.BusinessLogic.Dto.ApplicationDefinition
{
    public class GetFlowDefinitionsResponseDto
    {
        public IEnumerable<BasicFlowInfo> Flows { get; set; }
        public IDictionary<string, List<FlowSpecification>> Spec { get; set; }
    }

    public class BasicFlowInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Area { get; set; }
    }

    public class FlowSpecification
    {
        public string FlowName { get; set; }
        public string HttpMethod { get; set; }
        public string RequestUrl { get; set; }
        public string RequestBodyType { get; set; }
        public string RequestBodySchema { get; set; }
        public string ResponseBodySchema { get; set; }
        public List<FlowSpecificationType> Types { get; set; }
    }

    public class FlowSpecificationType
    {
        public string TypeName { get; set; }
        public List<string> TypeProperties { get; set; }
    }
}
