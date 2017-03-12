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
                var suffix = App.ResourceLoader.GetString("TimeSpanSuffix");
                var prefix = App.ResourceLoader.GetString("TimeSpanPrefix");
                if (span.Days > 365)
                {
                    int years = (span.Days / 365) -1;
                    if (span.Days % 365 != 0)
                        years += 1;
                    return $"{prefix} {years} {(years == 1 ? App.ResourceLoader.GetString("year") : App.ResourceLoader.GetString("years"))} {suffix}";
                }
                if (span.Days > 30)
                {
                    int months = (span.Days / 30) -1;
                    if (span.Days % 31 != 0)
                        months += 1;
                    return $"{prefix} {months} {(months == 1 ? App.ResourceLoader.GetString("month") : App.ResourceLoader.GetString("months"))} {suffix}";
                }
                if (span.Days > 0)
                    return $"{prefix} {span.Days} {(span.Days == 1 ? App.ResourceLoader.GetString("day") : App.ResourceLoader.GetString("days"))} {suffix}";
                if (span.Hours > 0)
                    return $"{prefix} {span.Hours} {(span.Hours == 1 ? App.ResourceLoader.GetString("hour") : App.ResourceLoader.GetString("hours"))} {suffix}";
                if (span.Minutes > 0)
                    return $"{prefix} {span.Minutes} {(span.Minutes == 1 ? App.ResourceLoader.GetString("minute") : App.ResourceLoader.GetString("minutes"))} {suffix}";
                if (span.Seconds > 5)
                    return $"{prefix} {span.Seconds} {(span.Seconds == 1 ? App.ResourceLoader.GetString("second") : App.ResourceLoader.GetString("seconds"))} {suffix}";
                if (span.Seconds <= 5)
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
