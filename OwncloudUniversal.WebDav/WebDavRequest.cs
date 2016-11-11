using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.SpeechRecognition;
using Windows.Security.Cryptography;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding;

namespace OwncloudUniversal.WebDav
{
    internal class WebDavRequest
    {
        private readonly HttpClient _httpClient;
        private readonly NetworkCredential _networkCredential;
        private readonly Uri _requestUrl;
        private readonly HttpMethod _method;
        private readonly Stream _contentStream;
        private const string PropfindContent = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><propfind xmlns=\"DAV:\"><allprop/></propfind>";

        public WebDavRequest(NetworkCredential networkCredential, Uri requestUrl, HttpMethod method, Stream contentStream = null)
        {
            _networkCredential = networkCredential;
            _requestUrl = requestUrl;
            _method = method;
            _contentStream = contentStream;
            _httpClient = new HttpClient();
        }

        public async Task<HttpResponseMessage> SendAsync()
        {
            using (var request = new HttpRequestMessage(_method, _requestUrl))
            {
                request.Headers.Connection.Add(new HttpConnectionOptionHeaderValue("Keep-Alive"));
                request.Headers.UserAgent.Add(HttpProductInfoHeaderValue.Parse("Mozilla/5.0"));

                var buffer = CryptographicBuffer.ConvertStringToBinary(_networkCredential.UserName + ":" + _networkCredential.Password, BinaryStringEncoding.Utf8);
                var token = CryptographicBuffer.EncodeToBase64String(buffer);
                request.Headers.Authorization = new HttpCredentialsHeaderValue("Basic", token);
                if(_method.Method == "PROPFIND")
                    request.Content = new HttpStringContent(PropfindContent, UnicodeEncoding.Utf8);
                else if(_contentStream != null)
                {
                   request.Content = new HttpStreamContent(_contentStream.AsInputStream()); 
                }
                var response = await _httpClient.SendRequestAsync(request, HttpCompletionOption.ResponseHeadersRead);
                if (!response.IsSuccessStatusCode)
                    throw new Exception(response.ReasonPhrase);//TODO better exceptions
                return response;
            }
        }
    }
}
