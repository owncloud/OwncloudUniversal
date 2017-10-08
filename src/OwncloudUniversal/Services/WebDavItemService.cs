using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.Security.Cryptography;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using OwncloudUniversal.Synchronization;
using OwncloudUniversal.Synchronization.LocalFileSystem;
using OwncloudUniversal.Synchronization.Model;
using OwncloudUniversal.OwnCloud;
using OwncloudUniversal.OwnCloud.Model;
using OwncloudUniversal.Synchronization.Configuration;

namespace OwncloudUniversal.Services
{
    class WebDavItemService
    {
        private static WebDavItemService _instance;
        private WebDavItemService()
        {
        }

        public static WebDavItemService GetDefault()
        {
            return _instance ?? (_instance = new WebDavItemService());
        }

        public async Task<List<DavItem>> GetItemsAsync(Uri folderHref)
        {
            var client = new WebDavClient(new Uri(Configuration.ServerUrl, UriKind.RelativeOrAbsolute), Configuration.Credential);
            var result = await client.ListFolder(CreateItemUri(folderHref));
            if (result.Count > 0) result.RemoveAt(0);
            return result.OrderBy(x => !x.IsCollection).ThenBy(x => x.DisplayName, StringComparer.CurrentCultureIgnoreCase).ToList();
        }

        private Uri CreateItemUri(Uri href)
        {
            if (string.IsNullOrWhiteSpace(Configuration.ServerUrl))
                return null;
            var serverUri = new Uri(Configuration.ServerUrl, UriKind.RelativeOrAbsolute);
            return new Uri(serverUri, href);
        }

        public async Task<StorageFile> DownloadAsync(BaseItem davItem, StorageFolder folder, CancellationToken token, Progress<HttpProgress> progress)
        {
            var client = new WebDavClient(new Uri(Configuration.ServerUrl, UriKind.RelativeOrAbsolute), Configuration.Credential);
            StorageFile file;
            Uri uri;
            if (davItem.IsCollection)
            {
                file = await folder.CreateFileAsync(davItem.DisplayName + ".zip", CreationCollisionOption.OpenIfExists);
                uri = new Uri(GetZipUrl(davItem.EntityId));
            }
            else
            {
                file = await folder.CreateFileAsync(davItem.DisplayName, CreationCollisionOption.OpenIfExists);
                uri = new Uri(davItem.EntityId, UriKind.RelativeOrAbsolute);
            }
            await client.Download(uri, file, token, progress);
            return file;
        }

        public async Task UploadAsync(DavItem item, StorageFile file, CancellationToken token, Progress<HttpProgress> progress)
        {   
            var uri = new Uri(item.EntityId.TrimEnd('/'), UriKind.RelativeOrAbsolute);
            uri = new Uri(uri + "/" + file.Name, UriKind.RelativeOrAbsolute);
            if (!uri.IsAbsoluteUri)
                uri = CreateItemUri(uri);
            var client = new WebDavClient(new Uri(Configuration.ServerUrl, UriKind.RelativeOrAbsolute), Configuration.Credential);
            await client.Upload(uri, file, token, progress);
        }

        public async Task DeleteItemAsync(List<DavItem> items)
        {
            var client = new WebDavClient(new Uri(Configuration.ServerUrl, UriKind.RelativeOrAbsolute), Configuration.Credential);
            foreach (var item in items)
            {
                await client.Delete(new Uri(item.EntityId, UriKind.RelativeOrAbsolute));
            }
        }

        public async Task CreateFolder(DavItem parentFolder, string folderName)
        {
            folderName = Uri.EscapeDataString(folderName);
            folderName = folderName.Replace("%28", "(");
            folderName = folderName.Replace("%29", ")");
            var uri = new Uri(parentFolder.EntityId.TrimEnd('/') + "/" + folderName, UriKind.RelativeOrAbsolute);

            var client = new WebDavClient(new Uri(Configuration.ServerUrl, UriKind.RelativeOrAbsolute), Configuration.Credential);
            await client.CreateFolder(uri);
        }

        public async Task MoveToFolder(DavItem itemToMove, DavItem targetFolder)
        {
            var client = new WebDavClient(new Uri(Configuration.ServerUrl, UriKind.RelativeOrAbsolute), Configuration.Credential);
            var target = targetFolder.EntityId.TrimEnd('/') + "/" + itemToMove.EntityId.TrimEnd('/').Substring(itemToMove.EntityId.TrimEnd('/').LastIndexOf('/') + 1);
            await client.Move(new Uri(itemToMove.EntityId, UriKind.RelativeOrAbsolute), new Uri(target, UriKind.RelativeOrAbsolute));
        }

        public async Task Rename(DavItem item, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                return;
            newName = Uri.EscapeDataString(newName);
            newName = newName.Replace("%28", "(");
            newName = newName.Replace("%29", ")");
            newName = item.EntityId.TrimEnd('/').Substring(0, item.EntityId.TrimEnd('/').LastIndexOf('/')+1) + newName;
            var client = new WebDavClient(new Uri(Configuration.ServerUrl, UriKind.RelativeOrAbsolute), Configuration.Credential);
            await client.Move(new Uri(item.EntityId, UriKind.RelativeOrAbsolute), new Uri(newName, UriKind.RelativeOrAbsolute));
        }

        private string GetZipUrl(string path)
        {
            string dirName = path.TrimEnd('/').Substring(path.TrimEnd('/').LastIndexOf('/')+1);
            int index = path.IndexOf("remote.php/webdav", StringComparison.OrdinalIgnoreCase);
            path = path.Substring(index + 18).Replace(dirName, "");
            path = WebUtility.UrlEncode(path);
            var url = Configuration.ServerUrl.Replace("remote.php/webdav", "index.php/apps/files/ajax/download.php?dir="+path+"&files="+dirName);
            
            
            return url;
        }
    }
}
