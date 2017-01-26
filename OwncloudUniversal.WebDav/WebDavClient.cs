using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Security.Credentials;
using Windows.Security.Cryptography;
using Windows.Storage;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using OwncloudUniversal.Shared;
using OwncloudUniversal.WebDav.Model;
using HttpStatusCode = Windows.Web.Http.HttpStatusCode;

namespace OwncloudUniversal.WebDav
{
    public class WebDavClient
    {
        private readonly Uri _serverUrl;
        private readonly NetworkCredential _credential;
        private readonly BackgroundDownloader _downloader;
        private readonly BackgroundUploader _uploader;

        public WebDavClient(Uri serverUrl, NetworkCredential credential)
        {
            _serverUrl = serverUrl;
            _credential = credential;

            BackgroundTransferGroup group = BackgroundTransferGroup.CreateGroup("owncloud-webdav-sync");
            group.TransferBehavior = BackgroundTransferBehavior.Serialized;

            _downloader = new BackgroundDownloader();
            _downloader.TransferGroup = group;
            _downloader.ServerCredential = new PasswordCredential(serverUrl.ToString(), credential.UserName, credential.Password);

            _uploader = new BackgroundUploader();
            _uploader.TransferGroup = group;
            _uploader.Method = "PUT";
            var buffer = CryptographicBuffer.ConvertStringToBinary(credential.UserName + ":" + credential.Password, BinaryStringEncoding.Utf8);
            var token = CryptographicBuffer.EncodeToBase64String(buffer);
            var value = new HttpCredentialsHeaderValue("Basic", token);
            _uploader.SetRequestHeader("Authorization", value.ToString());
        }

        public async Task<List<DavItem>> ListFolder(Uri url)
        {
            if (!url.IsAbsoluteUri)
                url = new Uri(_serverUrl, url);
            var propRequest = new WebDavRequest(_credential, url, new HttpMethod("PROPFIND"));
            var response = await propRequest.SendAsync();

            if (response.IsSuccessStatusCode)
            {
                var inputStream = await response.Content.ReadAsInputStreamAsync();
                return XmlParser.ParsePropfind(inputStream.AsStreamForRead());
            }
            throw new WebDavException(response.StatusCode, response.ReasonPhrase, null);
        }

        public async Task Download(Uri url, StorageFile targetFile)
        {
            if (!url.IsAbsoluteUri)
                url = new Uri(_serverUrl, url);
            var download = _downloader.CreateDownload(url, targetFile);
            await download.StartAsync();
        }

        public async Task<DavItem> Upload(Uri url, StorageFile file)
        {
            if (!url.IsAbsoluteUri)
                url = new Uri(_serverUrl, url);
            var upload = _uploader.CreateUpload(url, file);
            var task = upload.StartAsync().AsTask();
            var task2 = await task.ContinueWith(OnUploadCompleted);
            return await task2;
        }

        private async Task<DavItem> OnUploadCompleted(Task<UploadOperation> task)
        {
            var upload = await task;
            return (await ListFolder(upload.RequestedUri)).FirstOrDefault();
        }

        public async Task Delete(Uri url)
        {
            if(!url.IsAbsoluteUri)
                url = new Uri(_serverUrl, url);
            var delRequest = new WebDavRequest(_credential, url, HttpMethod.Delete);
            var response = await delRequest.SendAsync();
            if(response.IsSuccessStatusCode)
                return;
            throw new WebDavException(response.StatusCode, response.ReasonPhrase, null);
        }

        public async Task<DavItem> CreateFolder(Uri url)
        {
            if (!url.IsAbsoluteUri)
                url = new Uri(_serverUrl, url);
            var mkcolRequest = new WebDavRequest(_credential, url, new HttpMethod("MKCOL"));
            var response  = await mkcolRequest.SendAsync();
            if (response.IsSuccessStatusCode)
            {
                var items = await ListFolder(url);
                return items.FirstOrDefault();
            }
            throw new WebDavException(response.StatusCode, response.ReasonPhrase, null);
        }

        public async Task<bool> Exists(Uri url)
        {
            if (!url.IsAbsoluteUri)
                url = new Uri(_serverUrl, url);
            var delRequest = new WebDavRequest(_credential, url, HttpMethod.Head);
            var response = await delRequest.SendAsync();
            return response.StatusCode == HttpStatusCode.Ok;
        }
    }
}
