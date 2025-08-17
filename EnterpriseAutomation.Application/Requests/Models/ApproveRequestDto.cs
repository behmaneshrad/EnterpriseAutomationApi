namespace EnterpriseAutomation.Application.Requests.Models
{
    public class ApproveRequestDto
    {
        /// true -> تایید & false -> رد
        public bool IsApproved { get; set; }

        /// توضیحات/کامنت کاربر تأیید کننده
        public string? Comment { get; set; }
    }
}
