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

namespace OwncloudUniversal.Synchronization
{
    public static class LogHelper
    {
        public static async Task Write(string text)
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("log.txt",
                    CreationCollisionOption.OpenIfExists);
                text = DateTime.Now + " - " + text + Environment.NewLine;
                Debug.WriteLine(text);
                await Task.Run(() => File.AppendAllText(file.Path, text));
            }
            catch (Exception)
            {
            }
        }

        public static async void ResetLog()
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("log.txt",
                    CreationCollisionOption.OpenIfExists);

                var prop = await file.GetBasicPropertiesAsync();
                if (prop.Size > 1 * 1024 * 1024)
                    await file.DeleteAsync();
            }
            catch (Exception e)
            {
            }
        }
    }
}
