using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lowque.DataAccess.Entities.Identity
{
    public class User
    {
        [Key, Required]
        public int UserId { get; set; }

        [Required, MaxLength(100)]
        public string Email { get; set; }

        [Required]
        public bool EmailConfirmed { get; set; }

        [Required, MaxLength(100)]
        public string PasswordHash { get; set; }

        public virtual List<Role> Roles { get; set; }
    }
}
