using EnterpriseAutomation.Domain.Entities.Base;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace EnterpriseAutomation.Domain.Entities
{
    [Display(Name = "لاگ گردش کار")]
    public class WorkflowLog : BaseEntity
    {
        //[Key]
        //[Display(Name = "آی دی لاگ")]
        //public int Id { get; set; }

        [Display(Name = "آی دی مونگو")]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? _MongoId { get; set; }

        [Display(Name = "آی دی گردش کار")]
        public required int WorkflowId { get; set; }

        [Display(Name = "آی دی مرحله گردش کار")]
        public required int StepId { get; set; }

        [Display(Name = "آی دی درخواست دهنده")]
        public required Guid? UserId { get; set; }

        [Display(Name = "نام درخواست دهنده")]
        public required string UserName { get; set; }

        [Display(Name = "نوع عملیات")]
        public required string ActionType { get; set; }

        [Display(Name = "توضیحات")]
        public string ? Description { get; set; }

        [Display(Name = "شماره درخواست")]
        public required int RequestId { get; set; }

        [Display(Name = "وضعیت سابق")]
        public string? PreviousState { get; set; }

        [Display(Name = "وضعیت جدید")]
        public string? NewState { get; set; }

        [Display(Name = "تاریخ ثبت لاگ")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
        public override string ToString()
        {
            return JsonSerializer.Serialize(this ,new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy= JsonNamingPolicy.CamelCase
            });
        }
        
    }
}