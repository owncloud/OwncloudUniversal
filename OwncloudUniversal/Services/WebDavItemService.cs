using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Security.Credentials;
using Windows.Security.Cryptography;
using Windows.Storage;
using Windows.Web.Http.Headers;
using OwncloudUniversal.Shared;
using OwncloudUniversal.Shared.LocalFileSystem;
using OwncloudUniversal.Shared.Model;
using OwncloudUniversal.WebDav;
using OwncloudUniversal.WebDav.Model;

namespace OwncloudUniversal.Services
{
    class WebDavItemService
    {
        private static WebDavItemService _instance;
        private WebDavItemService()
        {
            DavAdapter = new WebDavAdapter(false, Configuration.ServerUrl, Configuration.Credential, null);
        }

        public static WebDavItemService GetDefault()
        {
            return _instance ?? (_instance = new WebDavItemService());
        }

        private WebDavAdapter DavAdapter { get; }

        public async Task<List<AbstractItem>> GetItemsAsync(Uri folderHref)
        {
            return await DavAdapter.GetAllItems(CreateItemUri(folderHref));
        }

        private Uri CreateItemUri(Uri href)
        {
            if (string.IsNullOrWhiteSpace(Configuration.ServerUrl))
                return null;
            var serverUri = new Uri(Configuration.ServerUrl, UriKind.RelativeOrAbsolute);
            return new Uri(serverUri, href);
        }

        public async Task<List<DownloadOperation>> CreateDownloadAsync(List<AbstractItem> items, StorageFolder folder)
        {
            List<DownloadOperation> result = new List<DownloadOperation>();
            BackgroundDownloader downloader = new BackgroundDownloader();
            downloader.ServerCredential = new PasswordCredential(Configuration.ServerUrl, Configuration.UserName, Configuration.Password);
            foreach (var davItem in items)
            {
                if(davItem.IsCollection)
                    continue;
                var file = await folder.CreateFileAsync(davItem.DisplayName, CreationCollisionOption.OpenIfExists);
                var uri = new Uri(davItem.EntityId, UriKind.RelativeOrAbsolute);
                result.Add(downloader.CreateDownload(CreateItemUri(uri), file));
            }
            return result;
        }

        public List<UploadOperation> CreateUpload(DavItem item, List<StorageFile> files)
        {
            List<UploadOperation> result = new List<UploadOperation>();
            BackgroundUploader uploader = new BackgroundUploader();
            uploader.Method = "PUT";
            var buffer = CryptographicBuffer.ConvertStringToBinary(Configuration.UserName + ":" + Configuration.Password, BinaryStringEncoding.Utf8);
            var token = CryptographicBuffer.EncodeToBase64String(buffer);
            var value = new HttpCredentialsHeaderValue("Basic", token);
            uploader.SetRequestHeader("Authorization", value.ToString());
            foreach (var storageFile in files)
            {
                var uri = new Uri(item.EntityId.TrimEnd('/'), UriKind.RelativeOrAbsolute);
                uri = new Uri(uri + "/" + storageFile.Name, UriKind.RelativeOrAbsolute);
                if (!uri.IsAbsoluteUri)
                    uri = CreateItemUri(uri);
                UploadOperation upload = uploader.CreateUpload(uri, storageFile);
                result.Add(upload);
            }
            return result;
        }

        public async Task DeleteItemAsync(DavItem item)
        {
            await DavAdapter.DavClient.Delete(new Uri(item.EntityId, UriKind.RelativeOrAbsolute));
        }


        public async Task CreateFolder(DavItem parentFolder, string folderName)
        {
            folderName += Uri.EscapeDataString(folderName);
            folderName = folderName.Replace("%28", "(");
            folderName = folderName.Replace("%29", ")");
            var uri = new Uri(parentFolder.EntityId.TrimEnd('/') + "/" + folderName, UriKind.RelativeOrAbsolute);
            await DavAdapter.DavClient.CreateFolder(uri);
        }
    }
}
