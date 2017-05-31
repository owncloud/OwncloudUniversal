using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace OwncloudUniversal.WebDav
{
    public class HttpRandomAccessStream : IRandomAccessStream
    {
        private readonly HttpClient _client;
        private IInputStream _inputStream;
        private ulong _size;
        private ulong _requestedPosition;
        private readonly Uri _requestedUri;
        private HttpRandomAccessStream(HttpClient client, Uri uri)
        {
            _client = client;
            _requestedUri = uri;
            _requestedPosition = 0;
        }

        public static IAsyncOperation<HttpRandomAccessStream> CreateAsync(HttpClient client, Uri uri)
        {
            return AsyncInfo.Run(async (cancellationToken) =>
            {
                HttpRandomAccessStream randomStream = new HttpRandomAccessStream(client, uri);
                await randomStream.SendRequestAsync().ConfigureAwait(false);
                return randomStream;
            });
        }

        private async Task SendRequestAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _requestedUri);
            request.Headers.Add("Range", string.Format("bytes={0}-", _requestedPosition));
            using (var response = await _client.SendRequestAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead).AsTask().ConfigureAwait(false))
            {
                if (response.Content.Headers.ContentLength != null)
                    _size = response.Content.Headers.ContentLength.Value;

                if (response.StatusCode != HttpStatusCode.PartialContent && _requestedPosition != 0)
                {
                    throw new Exception("HTTP server did not reply with a '206 Partial Content' status.");
                }
                if (response.Content.Headers.ContainsKey("Content-Type"))
                {
                    _contentType = response.Content.Headers["Content-Type"];
                }
                _inputStream = await response.Content.ReadAsInputStreamAsync();
            }
        }

        private string _contentType = string.Empty;

        public string ContentType => _contentType;

        public bool CanRead => true;

        public bool CanWrite => false;

        public IRandomAccessStream CloneStream()
        {
            return this;
        }

        public IInputStream GetInputStreamAt(ulong position)
        {
            throw new NotImplementedException();
        }

        public IOutputStream GetOutputStreamAt(ulong position)
        {
            throw new NotImplementedException();
        }

        public ulong Position => _requestedPosition;

        public void Seek(ulong position)
        {
            if (_requestedPosition != position)
            {
                if (_inputStream != null)
                {
                    _inputStream.Dispose();
                    _inputStream = null;
                }
                _requestedPosition = position;
            }
        }

        public ulong Size
        {
            get
            {
                return _size;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Dispose()
        {
            if (_inputStream != null)
            {
                _inputStream.Dispose();
                _inputStream = null;
            }
        }

        public IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(IBuffer buffer, uint count, InputStreamOptions options)
        {
            return AsyncInfo.Run<IBuffer, uint>(async (cancellationToken, progress) =>
            {
                progress.Report(0);
                if (_inputStream == null)
                {
                    await SendRequestAsync().ConfigureAwait(false);
                }
                IBuffer result = await _inputStream.ReadAsync(buffer, count, options).AsTask(cancellationToken, progress).ConfigureAwait(false);
                _requestedPosition += result.Length;
                return result;
            });
        }

        public IAsyncOperation<bool> FlushAsync()
        {
            throw new NotImplementedException();
        }

        public IAsyncOperationWithProgress<uint, uint> WriteAsync(IBuffer buffer)
        {
            throw new NotImplementedException();
        }
    }
}