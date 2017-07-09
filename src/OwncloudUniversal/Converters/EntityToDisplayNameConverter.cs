using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace OwncloudUniversal.Converters
{
    class EntityIdToDisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string name = "";
            if (value is Uri)
            {
                name = (value as Uri).ToString();
            }
            if (value is string)
            {
                name = (value as string);
            }
            if (name.Contains("webdav/"))
                name = name.Substring(name.TrimEnd('/').LastIndexOf("/", StringComparison.CurrentCultureIgnoreCase) + 1);
            else
            {
                name = name?.Substring(name.TrimEnd('\\').LastIndexOf('\\') + 1);
            }
            name = Uri.UnescapeDataString(name);
            name = name.Replace("%28", "(");
            name = name.Replace("%29", ")");
            return name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
