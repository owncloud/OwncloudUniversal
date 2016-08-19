using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwncloudUniversal.Shared.SQLite
{
    public static class DateTimeHelper
    {
        public static string DateTimeSQLite(DateTime? datetime)
        {
            string dateTimeFormat = "{0}-{1}-{2} {3}:{4}:{5}.{6}";
            return string.Format(dateTimeFormat, datetime.Value.Year, datetime.Value.Month, datetime.Value.Day, datetime.Value.Hour, datetime.Value.Minute, datetime.Value.Second, datetime.Value.Millisecond);
        }
    }
}
