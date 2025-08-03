namespace EnterpriseAutomation.Domain.Entities.Base
{
    public abstract class BaseEntity
    {
        protected BaseEntity()
        {
                this.CreatedAt = DateTime.Now;
        }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public int? UserCreatedId { get; set; } 
        public int? UserModifyId { get; set; }
    }
}