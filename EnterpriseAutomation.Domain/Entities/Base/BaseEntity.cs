namespace EnterpriseAutomation.Domain.Entities.Base
{
    public abstract class BaseEntity
    {
        protected BaseEntity()
        {
                this.CreatedAt = DateTime.Now;
        }
        public int UserCreatedId { get; set; }
        public DateTime CreatedAt { get; set; }
       
        public int UserModifyId { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
