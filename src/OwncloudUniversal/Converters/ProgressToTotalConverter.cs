using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.Web.Http;

namespace OwncloudUniversal.Converters
{
    class ProgressToTotalConverter : IValueConverter
    {
        private readonly BytesToSuffixConverter _byteConverter = new BytesToSuffixConverter();
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is HttpProgress progress)
            {
                if (progress.TotalBytesToReceive > 0)
                    return _byteConverter.Convert(progress.TotalBytesToReceive, null, null, string.Empty);

                if (progress.TotalBytesToSend > 0)
                    return _byteConverter.Convert(progress.TotalBytesToSend, null, null, string.Empty);
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
