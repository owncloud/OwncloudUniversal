using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using OwncloudUniversal.Shared;
using OwncloudUniversal.WebDav.Model;
using HttpMethod = Windows.Web.Http.HttpMethod;
using HttpStatusCode = Windows.Web.Http.HttpStatusCode;

namespace OwncloudUniversal.WebDav
{
    public class OcsClient
    {
        private readonly Uri _serverUrl;
        private readonly NetworkCredential _credential;

        public OcsClient(Uri serverUrl, NetworkCredential credential)
        {
            _serverUrl = serverUrl;
            _credential = credential;
        }

        public static async Task<ServerStatus> GetServerStatusAsync(string input)
        {
            var url = _BuildStatusUrl(input);
            if (url == null)
                return null;
            WebDavRequest request = new WebDavRequest(new NetworkCredential(string.Empty, string.Empty), url,
                HttpMethod.Get);
            try
            {
                var response = await request.SendAsync();
                var content = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Serverstatus response: {content}");
                var status = Newtonsoft.Json.JsonConvert.DeserializeObject<ServerStatus>(content);
                Configuration.ServerUrl = GetWebDavUrl(input);
                return status;
            }
            catch (Exception)
            {
                return null;
            }
            
        }

        private static Uri _BuildStatusUrl(string input)
        {
            if (Uri.IsWellFormedUriString(input, UriKind.Absolute))
            {
                input = input.TrimEnd().ToLower();
                if (input.EndsWith("owncloud") || input.EndsWith("nextcloud"))
                {
                    input += "/status.php";
                }
                if (input.EndsWith("remote.php/webdav"))
                {
                    input = input.Replace("remote.php/webdav", "status.php");
                }
                if (input.StartsWith("http://"))
                {
                    input = input.Replace("http://", "https://");
                }
                if (!(input.StartsWith("http://") || input.StartsWith("https://")))
                {
                    input = "https://" + input;
                }
                if (Uri.IsWellFormedUriString(input, UriKind.Absolute))
                {
                    return new Uri(input, UriKind.Absolute);
                }
                    
            }
            return null;
        }

        public async Task<bool> CheckUserLoginAsync()
        {
            var request = new WebDavRequest(_credential, _serverUrl, HttpMethod.Head);
            var response = await request.SendAsync();
            if (response.IsSuccessStatusCode)
                Configuration.ServerUrl = GetWebDavUrl(_serverUrl.ToString());
            return response.IsSuccessStatusCode;
        }

        private static string GetWebDavUrl(string url)
        {
            var statusUrl = _BuildStatusUrl(url);
            return statusUrl.ToString().Replace("status.php", "remote.php/webdav");
        }
    }
}
