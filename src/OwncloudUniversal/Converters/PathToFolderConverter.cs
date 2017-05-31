using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace OwncloudUniversal.Converters
{
    class PathToFolderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string name = "";
            if (value is string)
                name = value as string;
            if (name.Contains("webdav/"))
                name = name.Substring(name.IndexOf("webdav/", StringComparison.CurrentCultureIgnoreCase) + 6);
            name = Uri.UnescapeDataString(name);
            name = name.Replace("%28", "(");
            name = name.Replace("%29", ")");
            name = name.TrimEnd('/');
            name = name.Substring(0, name.LastIndexOf('/')+1);
            return name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
