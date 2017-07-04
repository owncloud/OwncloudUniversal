using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.SpeechRecognition;
using Windows.Security.Credentials;
using Windows.Security.Cryptography;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;
using HttpClient = Windows.Web.Http.HttpClient;
using HttpCompletionOption = Windows.Web.Http.HttpCompletionOption;
using HttpMethod = Windows.Web.Http.HttpMethod;
using HttpRequestMessage = Windows.Web.Http.HttpRequestMessage;
using HttpResponseMessage = Windows.Web.Http.HttpResponseMessage;
using UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding;

namespace OwncloudUniversal.OwnCloud
{
    internal class WebDavRequest
    {
        private readonly HttpClient _httpClient;
        private readonly NetworkCredential _networkCredential;
        private readonly Uri _requestUrl;
        private readonly HttpMethod _method;
        private readonly Stream _contentStream;
        private readonly Dictionary<string, string> _customHeaders;
        private const string PropfindContent = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><propfind xmlns=\"DAV:\"><allprop/></propfind>";

        /// <summary>
        /// Sends a WebDav/Http-Request to the Webdav-Server
        /// </summary>
        /// <param name="networkCredential"></param>
        /// <param name="requestUrl"></param>
        /// <param name="method"></param>
        /// <param name="contentStream">contentStream is not needed if sending a PROPFIND</param>
        /// <param name="customHeaders"></param>
        public WebDavRequest(NetworkCredential networkCredential, Uri requestUrl, HttpMethod method, Stream contentStream = null, Dictionary<string, string> customHeaders = null)
        {
            _networkCredential = networkCredential;
            _requestUrl = requestUrl;
            _method = method;
            _contentStream = contentStream;
            _customHeaders = customHeaders;
            var filter = new HttpBaseProtocolFilter {AllowUI = false};
            if(networkCredential.UserName != string.Empty && networkCredential.Password != string.Empty)
                filter.ServerCredential = new PasswordCredential(requestUrl.DnsSafeHost, networkCredential.UserName, networkCredential.Password);
            _httpClient = new HttpClient(filter);
        }

        public async Task<HttpResponseMessage> SendAsync()
        {
            using (var request = new HttpRequestMessage(_method, _requestUrl))
            using (_contentStream)
            {
                request.Headers.Connection.Add(new HttpConnectionOptionHeaderValue("Keep-Alive"));
                request.Headers.UserAgent.Add(HttpProductInfoHeaderValue.Parse("Mozilla/5.0"));
                if(_customHeaders != null)
                    foreach (var header in _customHeaders)
                        request.Headers.Add(header);
                
                if (_method.Method == "PROPFIND")
                    request.Content = new HttpStringContent(PropfindContent, UnicodeEncoding.Utf8);
                else if (_contentStream != null)
                {
                    request.Content = new HttpStreamContent(_contentStream.AsInputStream());
                }
                HttpResponseMessage response = await _httpClient.SendRequestAsync(request, HttpCompletionOption.ResponseHeadersRead);
                return response;
            }
        }
    }
}
