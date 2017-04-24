using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace OwncloudUniversal.Converters
{
    class DateTimeToDaysAgoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime)
            {
                TimeSpan span = DateTime.Now - (DateTime)value;
                if (span.Days > 365)
                {
                    int years = (span.Days / 365) -1;
                    if (span.Days % 365 != 0)
                        years += 1;
                    return string.Format(years == 1 ? App.ResourceLoader.GetString("AYearAgo") : App.ResourceLoader.GetString("XYearsAgo"), years);
                }
                if (span.Days > 30)
                {
                    int months = (span.Days / 30) -1;
                    if (span.Days % 31 != 0)
                        months += 1;
                    return string.Format(months == 1 ? App.ResourceLoader.GetString("AMonthAgo") : App.ResourceLoader.GetString("XMonthsAgo"), months);
                }
                if (span.Days > 0)
                    return string.Format(span.Days == 1 ? App.ResourceLoader.GetString("ADayAgo") : App.ResourceLoader.GetString("XDaysAgo"), span.Days);
                if (span.Hours > 0)
                    return string.Format(span.Hours == 1 ? App.ResourceLoader.GetString("AnHourAgo") : App.ResourceLoader.GetString("XHoursAgo"), span.Hours);
                if (span.Minutes > 0)
                    return string.Format(span.Minutes == 1 ? App.ResourceLoader.GetString("AMinuteAgo") : App.ResourceLoader.GetString("XMinutesAgo"), span.Minutes);
                if (span.Seconds > 5)
                    return string.Format(span.Seconds == 1 ? App.ResourceLoader.GetString("ASecondAgo") : App.ResourceLoader.GetString("XSecondsAgo"), span.Seconds);
                return App.ResourceLoader.GetString("justNow");
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
