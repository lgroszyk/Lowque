using System.ComponentModel.DataAnnotations;

namespace Lowque.DataAccess.Entities
{
    public class FlowDefinition
    {
        [Key, Required]
        public int FlowDefinitionId { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }

        [Required, MaxLength(50)]
        public string Area { get; set; }

        [Required, MaxLength(20)]
        public string Type { get; set; }

        [Required]
        public bool UseResourceIdentifier { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public ApplicationDefinition ApplicationDefinition { get; set; }

        [Required]
        public int ApplicationDefinitionId { get; set; }
    }
}
