using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Infrastructure.Utilities
{
    public static class PersianDateConverter
    {
        public static string ToShamsi(this DateTime date)
        {
            PersianCalendar persianCalendar = new PersianCalendar();
            return persianCalendar.GetYear(date) + "/"
                + persianCalendar.GetMonth(date).ToString("00")
                + "/" + persianCalendar.GetDayOfMonth(date).ToString("00");
        }
    }
}
