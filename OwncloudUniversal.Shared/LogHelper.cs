using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace OwncloudUniversal.Shared
{
    class LogHelper
    {
        public async void Write(string text)
        {
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("Owncloud-Sync-Log.txt", CreationCollisionOption.OpenIfExists);
            byte[] buffer = new byte[16 * 1024];
            using (var stream = await file.OpenStreamForWriteAsync())
            using (var content = new MemoryStream(Encoding.UTF8.GetBytes(text)))
            {
                int read = 0;
                while ((read = await content.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await stream.WriteAsync(buffer, 0, read);
                }
            }
        }
    }
}
