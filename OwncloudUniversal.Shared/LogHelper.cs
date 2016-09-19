using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace OwncloudUniversal.Shared
{
    class LogHelper
    {
        public async void Write(string text)
        {
            var folder = await StorageFolder.GetFolderFromPathAsync(@"C:\Data\Users\Public\Documents");
           
            var file = await folder.CreateFileAsync("Owncloud-Sync-Log.txt", CreationCollisionOption.OpenIfExists);
            byte[] buffer = new byte[16 * 1024];
            text = DateTime.Now + " - " + text + Environment.NewLine;
            await Task.Run(() => File.AppendAllText(file.Path, text));
        }
    }
}
