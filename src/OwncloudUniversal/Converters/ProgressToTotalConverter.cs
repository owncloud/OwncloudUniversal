using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.UI.Xaml.Data;

namespace OwncloudUniversal.Converters
{
    class ProgressToTotalConverter : IValueConverter
    {
        private readonly BytesToSuffixConverter _byteConverter = new BytesToSuffixConverter();
        public object Convert(object value, Type targetType, object parameter, string language)
        {

            if (value is BackgroundDownloadProgress)
            {
                var bytes = ((BackgroundDownloadProgress)value).TotalBytesToReceive;
                return _byteConverter.Convert(bytes, null, null, string.Empty);
            }
            if (value is BackgroundUploadProgress)
            {
                var bytes = ((BackgroundUploadProgress)value).TotalBytesToSend;
                return _byteConverter.Convert(bytes, null, null, string.Empty);
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
