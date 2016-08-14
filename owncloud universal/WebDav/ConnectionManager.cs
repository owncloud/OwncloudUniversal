using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Provider;
using owncloud_universal.Model;
using owncloud_universal.WebDav;

namespace owncloud_universal
{
    public static class ConnectionManager
    {
        private static WebDavClient _webDavClient = new WebDavClient();
        public static bool IsSetup;
        public static bool SetUp()
        {
            if (string.IsNullOrEmpty(Configuration.ServerUrl) || string.IsNullOrEmpty(Configuration.UserName) || string.IsNullOrEmpty(Configuration.Password))
                return false;
            _webDavClient = new WebDavClient(new NetworkCredential(Configuration.UserName, Configuration.Password))
            {
                Server = Configuration.ServerUrl,
                //BasePath = Configuration.FolderPath
            };
            IsSetup = true;
            return true;
        }
        public static async Task Upload(string href, Stream content, string fileName)
        {
            await _webDavClient.Upload(href, content, fileName);
        }
        public static async Task<List<RemoteItem>> GetFolder(string href)
        {
            if (string.IsNullOrWhiteSpace(href))
                href = Configuration.FolderPath;
            var davItems = await _webDavClient.List(href);
            return davItems.Select(davItem => new RemoteItem(davItem)).ToList();
        }
        public static async Task<bool> Download(string href, StorageFile localFile)
        {
            var content = await _webDavClient.Download(href);
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = content.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                await FileIO.WriteBytesAsync(localFile, ms.ToArray());
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(localFile);
                return status == FileUpdateStatus.Complete;
            }
        }
        public static async void DeleteFile(string href)
        {
            await _webDavClient.DeleteFile(href);
        }
        public static async void DeleteFolder(string path)
        {
            await _webDavClient.DeleteFolder(path);
        }
        public static async void CreateFolder(string href, string folderName)
        {
            try
            {
                await _webDavClient.CreateDir(href, folderName);
            }
            catch (Exception e)
            {

            }

        }
    }
}
