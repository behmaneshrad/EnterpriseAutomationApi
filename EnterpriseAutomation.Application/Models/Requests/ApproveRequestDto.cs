namespace EnterpriseAutomation.Application.Models.Requests
{
   public class ApproveRequestDto
    {
        //true -> تایید & false -> رد
        public bool IsApproved { get; set; }
        //توضیحات /کامنت کاربر تایید کننده
        public string? Comment { get; set; }
    }
}
