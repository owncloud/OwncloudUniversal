using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.Web.Http;

namespace OwncloudUniversal.Converters
{
    class ProgressToPercentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is HttpProgress progress)
            {
                if (progress.TotalBytesToReceive > 0)
                    return $"{Math.Round((decimal) (100 * progress.BytesReceived / progress.TotalBytesToReceive))} %";
                if (progress.TotalBytesToSend > 0)
                    return $"{Math.Round((decimal)(100 * progress.BytesSent / progress.TotalBytesToSend))} %";
            }
            return "0 %";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
