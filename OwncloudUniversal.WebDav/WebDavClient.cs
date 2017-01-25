using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;
using OwncloudUniversal.WebDav.Model;
using HttpStatusCode = Windows.Web.Http.HttpStatusCode;

namespace OwncloudUniversal.WebDav
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
            if(!url.IsAbsoluteUri)
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

        public async Task<Stream> Download(Uri url)
        {
            if (!url.IsAbsoluteUri)
                url = new Uri(_serverUrl, url);
            var getRequest = new WebDavRequest(_credential, url, HttpMethod.Get);
            var response = await getRequest.SendAsync();
            if (response.IsSuccessStatusCode)
            {
                var inputStream = await response.Content.ReadAsInputStreamAsync();
                return inputStream.AsStreamForRead(16*1024);
            }
            throw new WebDavException(response.StatusCode, response.ReasonPhrase, null);
        }

        public async Task<DavItem> Upload(Uri url, Stream contentStream)
        {
            if (!url.IsAbsoluteUri)
                url = new Uri(_serverUrl, url);
            var postRequest = new WebDavRequest(_credential, url, HttpMethod.Put, contentStream);
            var response = await postRequest.SendAsync();
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
    }
}
