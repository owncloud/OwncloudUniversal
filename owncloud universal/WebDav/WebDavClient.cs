using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using HttpClient = Windows.Web.Http.HttpClient;
using HttpMethod = Windows.Web.Http.HttpMethod;
using HttpRequestMessage = Windows.Web.Http.HttpRequestMessage;
using HttpResponseMessage = Windows.Web.Http.HttpResponseMessage;
using HttpStatusCode = Windows.Web.Http.HttpStatusCode;

namespace owncloud_universal.WebDav
{
    public class WebDavClient
    {
        private readonly NetworkCredential _credential;
        private static readonly HttpMethod PropFind = new HttpMethod("PROPFIND");
        private static readonly HttpMethod MoveMethod = new HttpMethod("MOVE");

        private static readonly HttpMethod MkCol = new HttpMethod("MKCOL");

        private const int HttpStatusCodeMultiStatus = 207;

        // http://webdav.org/specs/rfc4918.html#METHOD_PROPFIND
        private const string PropFindRequestContent =
            "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" +
            "<propfind xmlns=\"DAV:\">" +
            "<allprop/>" +
            //"  <propname/>" +
            //"  <prop>" +
            //"    <creationdate/>" +
            //"    <getlastmodified/>" +
            //"    <displayname/>" +
            //"    <getcontentlength/>" +
            //"    <getcontenttype/>" +
            //"    <getetag/>" +
            //"    <resourcetype/>" +
            //"  </prop> " +
            "</propfind>";
        
        private readonly HttpClient _client;
        private readonly HttpClient _uploadClient;
        private string _server;
        private string _basePath = "/";

        private string _encodedBasePath;
        


        #region WebDAV connection parameters

        /// <summary>
        /// Specify the WebDAV hostname (required).
        /// </summary>
        public string Server
        {
            get { return _server; }
            set
            {
                value = value.TrimEnd('/');
                _server = value;
            }
        }

        /// <summary>
        /// Specify the path of a WebDAV directory to use as 'root' (default: /)
        /// </summary>
        public string BasePath
        {
            get { return _basePath; }
            set
            {
                value = value.Trim('/');
                if (string.IsNullOrEmpty(value))
                    _basePath = "/";
                else
                    _basePath = "/" + value + "/";
            }
        }

        /// <summary>
        /// Specify an port (default: null = auto-detect)
        /// </summary>
        public int? Port { get; set; }

        /// <summary>
        /// Specify the UserAgent (and UserAgent version) string to use in requests
        /// </summary>
        public string UserAgent { get; set; }
        
        /// <summary>
        /// Specify the UserAgent (and UserAgent version) string to use in requests
        /// </summary>
        public string UserAgentVersion { get; set; }

        #endregion


        public WebDavClient(NetworkCredential credential = null, TimeSpan? uploadTimeout = null)
        {
            _credential = credential;
            _client = new HttpClient();
            _uploadClient = new HttpClient();
        }

        #region WebDAV operations

        /// <summary>
        /// List all files present on the server.
        /// </summary>
        /// <param name="path">List only files in this path</param>
        /// <param name="depth">Recursion depth</param>
        /// <returns>A list of files (entries without a trailing slash) and directories (entries with a trailing slash)</returns>
        public async Task<IEnumerable<DavItem>> List(string path = "/", int? depth = 1)
        {
            var listUri = await GetServerUrl(path, true).ConfigureAwait(false);

            // Depth header: http://webdav.org/specs/rfc4918.html#rfc.section.9.1.4
            IDictionary<string, string> headers = new Dictionary<string, string>();
            if (depth != null)
            {
                headers.Add("Depth", depth.ToString());
            }
            HttpResponseMessage response = null;
            try
            {
                response = await HttpRequest(listUri.Uri, PropFind, headers, Encoding.UTF8.GetBytes(PropFindRequestContent)).ConfigureAwait(false);

                if (response.StatusCode != HttpStatusCode.Ok &&
                    (int) response.StatusCode != HttpStatusCodeMultiStatus)
                {
                    throw new Exception("Failed retrieving items in folder.");
                }

                using (var stream = await response.Content.ReadAsInputStreamAsync())
                {
                    var items = ResponseParser.ParseItems(stream.AsStreamForRead());

                    if (items == null)
                    {
                        throw new Exception("Failed deserializing data returned from server.");
                    }

                    var listUrl = listUri.ToString();

                    var result = new List<DavItem>(items.Count());
                    foreach (var item in items)
                    {
                        if(depth == 0)
                        {
                            result.Add(item);
                            continue;
                        }
                        // If it's not a collection, add it to the result
                        if (!item.IsCollection)
                        {
                            result.Add(item);
                        }
                        else
                        {
                            // If it's not the requested parent folder, add it to the result
                            var fullHref = await GetServerUrl(item.Href, true).ConfigureAwait(false);
                            if (!string.Equals(fullHref.ToString(), listUrl, StringComparison.CurrentCultureIgnoreCase))
                            {
                                result.Add(item);
                            }
                        }
                    }
                    return result;
                }

            }
            finally
            {
                if (response != null)
                    response.Dispose();
            }
        }

        /// <summary>
        /// List all files present on the server.
        /// </summary>
        /// <returns>A list of files (entries without a trailing slash) and directories (entries with a trailing slash)</returns>
        public async Task<DavItem> GetFolder(string path = "/")
        {
            var listUri = await GetServerUrl(path, true).ConfigureAwait(false);
            return await Get(listUri.Uri, path).ConfigureAwait(false);
        }

        /// <summary>
        /// List all files present on the server.
        /// </summary>
        /// <returns>A list of files (entries without a trailing slash) and directories (entries with a trailing slash)</returns>
        public async Task<DavItem> GetFile(string path = "/")
        {
            var listUri = await GetServerUrl(path, false).ConfigureAwait(false);
            return await Get(listUri.Uri, path).ConfigureAwait(false);
        }


        /// <summary>
        /// List all files present on the server.
        /// </summary>
        /// <returns>A list of files (entries without a trailing slash) and directories (entries with a trailing slash)</returns>
        private async Task<DavItem> Get(Uri listUri, string path)
        {

            // Depth header: http://webdav.org/specs/rfc4918.html#rfc.section.9.1.4
            IDictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Depth", "0");
            HttpResponseMessage response = null;
            try
            {
                response = await HttpRequest(listUri, PropFind, headers, Encoding.UTF8.GetBytes(PropFindRequestContent)).ConfigureAwait(false);

                if (response.StatusCode != HttpStatusCode.Ok &&
                    (int) response.StatusCode != HttpStatusCodeMultiStatus)
                {
                    throw new Exception(string.Format("Failed retrieving item/folder (Status Code: {0})", response.StatusCode));
                }

                using (var stream = await response.Content.ReadAsInputStreamAsync())
                {
                    var result = ResponseParser.ParseItem(stream.AsStreamForRead());

                    if (result == null)
                    {
                        throw new Exception("Failed deserializing data returned from server.");
                    }

                    return result;
                }
            }
            finally
            {
                if (response != null)
                    response.Dispose();
            }
        }

        /// <summary>
        /// Download a file from the server
        /// </summary>
        /// <param name="remoteFilePath">Source path and filename of the file on the server</param>
        public async Task<Stream> Download(string remoteFilePath)
        {
            // Should not have a trailing slash.
            var downloadUri = await GetServerUrl(remoteFilePath, false).ConfigureAwait(false);

            var dictionary = new Dictionary<string, string> { { "translate", "f" } };
            var response = await HttpRequest(downloadUri.Uri, HttpMethod.Get, dictionary).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.Ok)
            {
                throw new Exception("Failed retrieving file.");
            }
            return ((IInputStream)await response.Content.ReadAsInputStreamAsync()).AsStreamForRead();
        }

        /// <summary>
        /// Download a file from the server
        /// </summary>
        /// <param name="remoteFilePath">Source path and filename of the file on the server</param>
        /// <param name="content"></param>
        /// <param name="name"></param>
        public async Task<bool> Upload(string remoteFilePath, Stream content, string name)
        {
            // Should not have a trailing slash.
            var uploadUri = await GetServerUrl(remoteFilePath.TrimEnd('/') + "/" + name.TrimStart('/'), false).ConfigureAwait(false);

            HttpResponseMessage response = null;

            try
            {
                response = await HttpUploadRequest(uploadUri.Uri, HttpMethod.Put, content).ConfigureAwait(false);

                if (response.StatusCode != HttpStatusCode.Ok &&
                    response.StatusCode != HttpStatusCode.NoContent &&
                    response.StatusCode != HttpStatusCode.Created)
                {
                    throw new Exception("Failed uploading file.");
                }

                return response.IsSuccessStatusCode;
            }
            finally
            {
                if (response != null)
                    response.Dispose();
            }

        }


        /// <summary>
        /// Create a directory on the server
        /// </summary>
        /// <param name="remotePath">Destination path of the directory on the server</param>
        /// <param name="name"></param>
        public async Task<bool> CreateDir(string remotePath, string name)
        {
            // Should not have a trailing slash.
            var dirUri = await GetServerUrl(remotePath.TrimEnd('/') + "/" + name.TrimStart('/'), false).ConfigureAwait(false);

            HttpResponseMessage response = null;

            try
            {
                response = await HttpRequest(dirUri.Uri, MkCol).ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.Conflict)
                    throw new Exception("Failed creating folder.");

                if (response.StatusCode != HttpStatusCode.Ok &&
                    response.StatusCode != HttpStatusCode.NoContent &&
                    response.StatusCode != HttpStatusCode.Created)
                {
                    throw new Exception("Failed creating folder.");
                }

                return response.IsSuccessStatusCode;
            }
            finally
            {
                if (response!= null)
                    response.Dispose();
            }
        }

        public async Task DeleteFolder(string href)
        {
            var listUri = await GetServerUrl(href, true).ConfigureAwait(false);
            await Delete(listUri.Uri).ConfigureAwait(false);
        }

        public async Task DeleteFile(string href)
        {
            var listUri = await GetServerUrl(href, false).ConfigureAwait(false);
            await Delete(listUri.Uri).ConfigureAwait(false);
        }


        private async Task Delete(Uri listUri)
        {
            var response = await HttpRequest(listUri, HttpMethod.Delete).ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.Ok &&
                response.StatusCode != HttpStatusCode.NoContent)
            {
                throw new Exception("Failed deleting item.");
            }
        }

        public async Task<bool> MoveFolder(string srcFolderPath, string dstFolderPath)
        {
            // Should have a trailing slash.
            var srcUri = await GetServerUrl(srcFolderPath, true).ConfigureAwait(false);
            var dstUri = await GetServerUrl(dstFolderPath, true).ConfigureAwait(false);

            return await Move(srcUri.Uri, dstUri.Uri).ConfigureAwait(false);

        }

        public async Task<bool> MoveFile(string srcFilePath, string dstFilePath)
        {
            // Should not have a trailing slash.
            var srcUri = await GetServerUrl(srcFilePath, false).ConfigureAwait(false);
            var dstUri = await GetServerUrl(dstFilePath, false).ConfigureAwait(false);

            return await Move(srcUri.Uri, dstUri.Uri).ConfigureAwait(false);
        }


        private async Task<bool> Move(Uri srcUri, Uri dstUri)
        {
            const string requestContent = "MOVE";

            IDictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Destination", dstUri.ToString());

            var response = await HttpRequest(srcUri, MoveMethod, headers, Encoding.UTF8.GetBytes(requestContent)).ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.Ok &&
                response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception("Failed moving file.");
            }

            return response.IsSuccessStatusCode;
        }

        #endregion

        #region Server communication

        /// <summary>
        /// Perform the WebDAV call and fire the callback when finished.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="method"></param>
        /// <param name="headers"></param>
        /// <param name="content"></param>
        private async Task<HttpResponseMessage> HttpRequest(Uri uri, Windows.Web.Http.HttpMethod method, IDictionary<string, string> headers = null, byte[] content = null)
        {
            using (HttpRequestMessage request = new Windows.Web.Http.HttpRequestMessage(method, uri))
            {
                request.Headers.Connection.Add(new HttpConnectionOptionHeaderValue("Keep-Alive"));
                request.Headers.UserAgent.Add(HttpProductInfoHeaderValue.Parse("Mozilla/4.0"));

                var buffer = Windows.Security.Cryptography.CryptographicBuffer.ConvertStringToBinary(_credential.UserName + ":" + _credential.Password, Windows.Security.Cryptography.BinaryStringEncoding.Utf8);
                string base64token = Windows.Security.Cryptography.CryptographicBuffer.EncodeToBase64String(buffer);

                request.Headers.Authorization = new HttpCredentialsHeaderValue("Basic", base64token);
                if (headers != null)
                {
                    foreach (string key in headers.Keys)
                    {
                        request.Headers.Add(key, headers[key]);
                    }
                }

                // Need to send along content?
                if (content != null)
                {
                    request.Content = new HttpStreamContent(new MemoryStream(content).AsRandomAccessStream());
                    request.Content.Headers.ContentType = new HttpMediaTypeHeaderValue("text/xml");
                }
                
                return await _client.SendRequestAsync(request);
            }
        }

        /// <summary>
        /// Perform the WebDAV call and fire the callback when finished.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="headers"></param>
        /// <param name="method"></param>
        /// <param name="content"></param>
        private async Task<HttpResponseMessage> HttpUploadRequest(Uri uri, HttpMethod method, Stream content, IDictionary<string, string> headers = null)
        {
            using (var request = new HttpRequestMessage(method, uri))
            {
                request.Headers.Connection.Add(HttpConnectionOptionHeaderValue.Parse("Keep-Alive"));
                request.Headers.UserAgent.Add(!string.IsNullOrWhiteSpace(UserAgent)
                    ? new HttpProductInfoHeaderValue(UserAgent, UserAgentVersion)
                    : new HttpProductInfoHeaderValue("Webdav"));

                var b = Windows.Security.Cryptography.CryptographicBuffer.ConvertStringToBinary(_credential.UserName + ":" + _credential.Password, Windows.Security.Cryptography.BinaryStringEncoding.Utf16LE);
                string base64token = Windows.Security.Cryptography.CryptographicBuffer.EncodeToBase64String(b);

                request.Headers.Authorization = new HttpCredentialsHeaderValue("Basic", base64token);
                if (headers != null)
                {
                    foreach (string key in headers.Keys)
                    {
                        request.Headers.Add(key, headers[key]);
                    }
                }

                // Need to send along content?
                if (content != null)
                {
                    byte[] buffer = new byte[16*1024];
                    MemoryStream ms = new MemoryStream();

                    int read;
                    while ((read = content.Read(buffer, 0, buffer.Length)) > 0)
                        ms.Write(buffer, 0, read);
                    request.Content = new HttpBufferContent(ms.GetWindowsRuntimeBuffer());
                }
                var client = _uploadClient ?? _client;
                var response =  await client.SendRequestAsync(request);
                return response;
            }
        }

        /// <summary>
        /// Try to create an Uri with kind UriKind.Absolute
        /// This particular implementation also works on Mono/Linux 
        /// It seems that on Mono it is expected behaviour that uris
        /// of kind /a/b are indeed absolute uris since it referes to a file in /a/b. 
        /// https://bugzilla.xamarin.com/show_bug.cgi?id=30854
        /// </summary>
        /// <param name="uriString"></param>
        /// <param name="uriResult"></param>
        /// <returns></returns>
        private static bool TryCreateAbsolute(string uriString, out Uri uriResult)
        {
            return Uri.TryCreate(uriString, UriKind.Absolute, out uriResult);
        }

        private async Task<UriBuilder> GetServerUrl(string path, bool appendTrailingSlash)
        {
            // Resolve the base path on the server
            if (_encodedBasePath == null)
            {
                var baseUri = new UriBuilder(_server) {Path = _basePath};
                var root = await Get(baseUri.Uri, null).ConfigureAwait(false);

                _encodedBasePath = root.Href;
            }


            // If we've been asked for the "root" folder
            if (string.IsNullOrEmpty(path))
            {
                // If the resolved base path is an absolute URI, use it
                Uri absoluteBaseUri;
                if (TryCreateAbsolute(_encodedBasePath, out absoluteBaseUri))
                {
                    return new UriBuilder(absoluteBaseUri);
                }

                // Otherwise, use the resolved base path relatively to the server
                var baseUri = new UriBuilder(_server) {Path = _encodedBasePath};
                return baseUri;
            }

            // If the requested path is absolute, use it
            Uri absoluteUri;
            if (TryCreateAbsolute(path, out absoluteUri))
            {
                var baseUri = new UriBuilder(absoluteUri);
                return baseUri;
            }
            else
            {
                // Otherwise, create a URI relative to the server
                UriBuilder baseUri;
                if (TryCreateAbsolute(_encodedBasePath, out absoluteUri))
                {
                    baseUri = new UriBuilder(absoluteUri);

                    baseUri.Path = baseUri.Path.TrimEnd('/') + "/" + path.TrimStart('/');

                    if (appendTrailingSlash && !baseUri.Path.EndsWith("/"))
                        baseUri.Path += "/";
                }
                else
                {
                    baseUri = new UriBuilder(_server);

                    // Ensure we don't add the base path twice
                    var finalPath = path;
                    if (!finalPath.StartsWith(_encodedBasePath, StringComparison.CurrentCultureIgnoreCase))
                    {
                        finalPath = _encodedBasePath.TrimEnd('/') + "/" + path;
                    }
                    if (appendTrailingSlash)
                        finalPath = finalPath.TrimEnd('/') + "/";

                    baseUri.Path = finalPath;
                }
                

                return baseUri;
            }
        }

        #endregion
    }
}
