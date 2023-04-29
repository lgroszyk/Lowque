using System.Collections.Generic;

namespace Lowque.BusinessLogic.Dto.ApplicationDefinition
{
    public class GetExistingApplicationsAndTemplatesResponseDto
    {
        public IEnumerable<BasicAppInfo> Applications { get; set; }
        public IEnumerable<string> Templates { get; set; }
    }

    public class BasicAppInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CreatedAt { get; set; }
    }
}
