using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Streams;
using Windows.UI.Xaml;

namespace OwncloudUniversal.Shared
{
    public static class LogHelper
    {
        public static async Task Write(string text)
        {
            try
            {
                Debug.WriteLine(text);
                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("log.txt",
                    CreationCollisionOption.OpenIfExists);
                text = DateTime.Now + " - " + text + Environment.NewLine;
                await Task.Run(() => File.AppendAllText(file.Path, text));
            }
            catch (Exception)
            {
            }
        }
    }
}
