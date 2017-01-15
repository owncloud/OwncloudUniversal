using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.UI.Xaml.Data;

namespace OwncloudUniversal.Converters
{
    class ProgressToPercentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is BackgroundDownloadProgress)
            {
                if (((BackgroundDownloadProgress)value).TotalBytesToReceive != 0)
                    return $"{Math.Round((decimal) (100*((BackgroundDownloadProgress) value).BytesReceived) / ((BackgroundDownloadProgress) value).TotalBytesToReceive)} %";
            }
            if (value is BackgroundUploadProgress)
            {
                if(((BackgroundUploadProgress)value).TotalBytesToSend != 0)
                    return $"{Math.Round((decimal)(100 * ((BackgroundUploadProgress)value).BytesSent) / ((BackgroundUploadProgress)value).TotalBytesToSend)} %";
            }
            return "0 %";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
