using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lowque.DataAccess.Entities
{
    public class ApplicationDefinition
    {
        [Key]
        [Required]
        public int ApplicationDefinitionId { get; set; }

        [StringLength(50, MinimumLength = 3)]
        [Required]
        public string Name { get; set; }

        [StringLength(50, MinimumLength = 3)]
        public string Template { get; set; }

        [Required]
        public bool IsTemplate { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public virtual List<FlowDefinition> BusinessLogicModuleDefinition { get; set; }

        [Required]
        public string DataAccessModuleDefinition { get; set; }

        [Required]
        public string IdentityModuleDefinition { get; set; }
    }
}
