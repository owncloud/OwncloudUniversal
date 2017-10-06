using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.Security.Cryptography;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;
using OwncloudUniversal.OwnCloud.Model;
using OwncloudUniversal.Synchronization;
using OwncloudUniversal.Synchronization.Processing;
using HttpStatusCode = Windows.Web.Http.HttpStatusCode;

namespace OwncloudUniversal.OwnCloud
{
    public class WebDavClient
    {
        private readonly Uri _serverUrl;
        private readonly NetworkCredential _credential;

        public WebDavClient(Uri serverUrl, NetworkCredential credential)
        {
            _serverUrl = serverUrl;
            _credential = credential;
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
            Progress<HttpProgress> progressCallback = new Progress<HttpProgress>(async progress => await OnHttpProgressChanged(progress));
            WebDavRequest request = new WebDavRequest(_credential, url, HttpMethod.Get);
            var tokenSource = new CancellationTokenSource();
            var response = await request.SendAsync(tokenSource.Token, progressCallback);
            if (response.IsSuccessStatusCode)
            {
                var inputStream = await response.Content.ReadAsInputStreamAsync();
                byte[] buffer = new byte[16 * 1024];
                using (var writingStream = await targetFile.OpenStreamForWriteAsync())
                using (var readingStream = inputStream.AsStreamForRead(16 * 1024))
                {
                    int read = 0;
                    while ((read = await readingStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await writingStream.WriteAsync(buffer, 0, read);
                    }
                }
            }
            else
            {
                throw new WebDavException(response.StatusCode, response.ReasonPhrase, null);
            }
            
        }

        private Task OnHttpProgressChanged(HttpProgress progress)
        {
            if (progress.TotalBytesToReceive > 0)
            {
                Debug.WriteLine($"{progress.BytesReceived} / {progress.TotalBytesToReceive} - {progress.Stage}");
            }
            if (progress.TotalBytesToSend > 0)
            {
                Debug.WriteLine($"{progress.BytesSent} / {progress.TotalBytesToSend} - {progress.Stage}");
            }
            return Task.CompletedTask;
        }
        

        public async Task<DavItem> Upload(Uri url, StorageFile file)
        {
            if (!url.IsAbsoluteUri)
                url = new Uri(_serverUrl, url);
            var stream = await file.OpenStreamForReadAsync();
            Progress<HttpProgress> progressCallback = new Progress<HttpProgress>(async progress => await OnHttpProgressChanged(progress));
            var tokenSource = new CancellationTokenSource();
            var postRequest = new WebDavRequest(_credential, url, HttpMethod.Put, stream);
            var response = await postRequest.SendAsync(tokenSource.Token, progressCallback);
            if (response.IsSuccessStatusCode)
            {
                var items = await ListFolder(url);
                return items.FirstOrDefault();
            }
            throw new WebDavException(response.StatusCode, response.ReasonPhrase, null);
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

        public async Task Move(Uri sourceUrl, Uri destinationUrl)
        {
            if (!sourceUrl.IsAbsoluteUri)
                sourceUrl = new Uri(_serverUrl, sourceUrl);
            if (!destinationUrl.IsAbsoluteUri)
                destinationUrl = new Uri(_serverUrl, destinationUrl);
            var headers = new Dictionary<string, string> {{ "Destination", destinationUrl.ToString()}};
            var moveRequest = new WebDavRequest(_credential, sourceUrl, new HttpMethod("MOVE"), Stream.Null, headers);
            var response = await moveRequest.SendAsync();
            if(response.IsSuccessStatusCode)
                return;
            throw new WebDavException(response.StatusCode, response.ReasonPhrase, null);
        }
    }
}
