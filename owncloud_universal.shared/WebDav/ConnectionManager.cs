using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Provider;
using System.Diagnostics;
using OwncloudUniversal.Shared.Model;

namespace OwncloudUniversal.Shared.WebDav
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
            if (!IsSetup)
                SetUp();
            await _webDavClient.Upload(href, content.AsInputStream(), fileName);
        }
        public static async Task<List<RemoteItem>> GetFolder(string href)
        {
            if (!IsSetup)
                SetUp();
            if (string.IsNullOrWhiteSpace(href))
                href = Configuration.FolderPath;
            var davItems = await _webDavClient.List(href);
            return davItems.Where(davItem => davItem.Href != href ).Select(x => new RemoteItem(x)).ToList();
        }
        public static async Task<bool> Download(string href, StorageFile localFile)
        {
            if (!IsSetup)
                SetUp();
            byte[] buffer = new byte[16 * 1024 * 1024];
            using (var stream = await localFile.OpenStreamForWriteAsync())
            using (var content = await _webDavClient.Download(href))
            {
                int read = 0;
                while ((read = await content.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await stream.WriteAsync(buffer, 0, read);
                }
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(localFile);
                return status == FileUpdateStatus.Complete;
            }
        }
        public static async void DeleteFile(string href)
        {
            if (!IsSetup)
                SetUp();

            await _webDavClient.DeleteFile(href);
        }
        public static async void DeleteFolder(string path)
        {
            if (!IsSetup)
                SetUp();

            await _webDavClient.DeleteFolder(path);
        }
        public static async Task CreateFolder(string href, string folderName)
        {
            try
            {
                if (!IsSetup)
                    SetUp();
                await _webDavClient.CreateDir(href, folderName);
            }
            catch (Exception e)
            {
                Debug.WriteLine(string.Format("Error: {0}, Href: {1}, folderName: {3}", e.Message, href, folderName));
            }

        }
    }
}
