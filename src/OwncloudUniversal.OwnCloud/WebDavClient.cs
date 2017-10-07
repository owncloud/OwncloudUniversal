using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Security.Credentials;
using Windows.Security.Cryptography;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;
using OwncloudUniversal.OwnCloud.Model;
using OwncloudUniversal.Synchronization;
using OwncloudUniversal.Synchronization.Model;
using OwncloudUniversal.Synchronization.Processing;
using Buffer = Windows.Storage.Streams.Buffer;
using ExecutionContext = OwncloudUniversal.Synchronization.Processing.ExecutionContext;
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

        public async Task Download(Uri url, StorageFile targetFile, CancellationToken token, Progress<HttpProgress> progress)
        {
            if (!url.IsAbsoluteUri)
                url = new Uri(_serverUrl, url);
            WebDavRequest request = new WebDavRequest(_credential, url, HttpMethod.Get);
            var response = await request.SendAsync(token, progress);
            if (response.IsSuccessStatusCode)
            {
                var inputStream = await response.Content.ReadAsInputStreamAsync();
                IBuffer buffer = new Buffer(16*1024);
               
                using (var writingStream = await targetFile.OpenStreamForWriteAsync())
                {
                    ulong received = 0;
                    do
                    {
                        buffer = await inputStream.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.ReadAhead).AsTask(token);
                        received += buffer.Length;
                        await writingStream.WriteAsync(buffer.ToArray(), 0, (int)buffer.Length, token);
                        ((IProgress<HttpProgress>)progress).Report(new HttpProgress
                        {
                            BytesReceived = received,
                            TotalBytesToReceive = response.Content.Headers.ContentLength,
                            Stage = HttpProgressStage.ReceivingContent
                        });
                        if(token.IsCancellationRequested)
                            return;
                    } while (buffer.Length > 0);
                }
            }
            else
            {
                throw new WebDavException(response.StatusCode, response.ReasonPhrase, null);
            }
            
        }
        
        public async Task<DavItem> Upload(Uri url, StorageFile file, CancellationToken token, Progress<HttpProgress> progress)
        {
            if (!url.IsAbsoluteUri)
                url = new Uri(_serverUrl, url);
            var stream = await file.OpenStreamForReadAsync();
            var postRequest = new WebDavRequest(_credential, url, HttpMethod.Put, stream);
            var response = await postRequest.SendAsync(token, progress);
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
