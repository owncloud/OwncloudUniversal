using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace OwncloudUniversal.Converters
{
    class UriToNicePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string name = "";
            if (value is Uri)
            {
                name = (value as Uri).ToString();
                if(name.Contains("webdav/"))
                    name = name.Substring(name.IndexOf("webdav/", StringComparison.CurrentCultureIgnoreCase)+6);
            }
            if (value is string)
            {
                name = (value as string);
                if (name.Contains("webdav/"))
                    name = name.Substring(name.IndexOf("webdav/", StringComparison.CurrentCultureIgnoreCase) + 6);
            }
            return name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
