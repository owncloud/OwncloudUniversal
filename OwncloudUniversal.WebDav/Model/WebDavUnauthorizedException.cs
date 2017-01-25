using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace OwncloudUniversal.WebDav.Model
{
    public class WebDavException : HttpRequestException
    {
        /// <summary>
        /// The HTTP status code of the server response
        /// </summary>
        public HttpStatusCode HttpStatusCode { get; }
        /// <summary>
        /// Inititalizes a new instance of WebDavExption
        /// </summary>
        /// <param name="httpCode">The HTTP code of the server response</param>
        /// <param name="message">The ReasonPhrase of the server response</param>
        /// <param name="innerException">The inner Exception</param>
        public WebDavException(HttpStatusCode httpCode, string message, Exception innerException) : base(message, innerException)
        {
        }

    }
}
