using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace OwncloudUniversal.WebDav
{
    internal class WebDavClient
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
            var inputStream = await response.Content.ReadAsInputStreamAsync();
            return XmlParser.ParsePropfind(inputStream.AsStreamForRead());
        }

        public async Task<Stream> Download(Uri url)
        {
            if (!url.IsAbsoluteUri)
                url = new Uri(_serverUrl, url);
            var getRequest = new WebDavRequest(_credential, url, HttpMethod.Get);
            var response = await getRequest.SendAsync();
            var inputStream = await response.Content.ReadAsInputStreamAsync();
            return inputStream.AsStreamForRead();
        }

        public async Task<DavItem> Upload(Uri url, Stream contentStream)
        {
            if (!url.IsAbsoluteUri)
                url = new Uri(_serverUrl, url);
            var postRequest = new WebDavRequest(_credential, url, HttpMethod.Put, contentStream);
            var postResponse = await postRequest.SendAsync();
            var items = await ListFolder(url);
            return items.FirstOrDefault();
        }

        public void Delete()
        {
            
        }

        public async Task<DavItem> CreateFolder(Uri url)
        {
            if (!url.IsAbsoluteUri)
                url = new Uri(_serverUrl, url);
            var mkcolRequest = new WebDavRequest(_credential, url, new HttpMethod("MKCOL"));
            await mkcolRequest.SendAsync();
            var items = await ListFolder(url);
            return items.FirstOrDefault();
        }

        public void DeleteFolder()
        {
            
        }
        
    }
}
