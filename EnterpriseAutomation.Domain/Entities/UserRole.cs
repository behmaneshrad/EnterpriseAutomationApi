using EnterpriseAutomation.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Domain.Entities
{
    [Display(Name = "نقش کاربران")]
    public class UserRole : BaseEntity
    {
        [Key]
        public int UserRoleId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public virtual User User { get; set; } = default!;
        public virtual Role Role { get; set; } = default!;
    }
}
