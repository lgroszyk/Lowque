using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lowque.DataAccess.Entities.Identity
{
    public class Role
    {
        [Key, Required]
        public int RoleId { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }

        public virtual List<User> Users { get; set; }
    }
}
