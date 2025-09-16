using EnterpriseAutomation.Domain.Entities.Base;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseAutomation.Domain.Entities
{
    [Display(Name = "نقش")]
    public class Role : BaseEntity
    {
        [Key]
        [Display(Name = "آیدی نقش")]
        public int RoleId { get; set; }

        [Display(Name = "نام نقش")]
        public string RoleName { get; set; } = string.Empty;


        public virtual ICollection<User> Users { get; set; } = [];
        public ICollection<RolePermissions> RolePermissions { get; set; } = new List<RolePermissions>();
        public virtual ICollection<UserRole> UserRoles { get; set; } = [];

    }
}
