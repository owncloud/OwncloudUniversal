using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace OwncloudUniversal.Shared
{
    public static class Configuration
    {
        private static Windows.Storage.ApplicationDataContainer _config = Windows.Storage.ApplicationData.Current.LocalSettings;

        public static string ServerUrl
        {
            get
            {
                if (_config.Values.ContainsKey("ServerUrl"))
                    return (string)_config.Values["ServerUrl"];
                return String.Empty;
            }
            set { _config.Values["ServerUrl"] = value; } 
        }

        public static string FolderPath
        {
            get
            {
                if (_config.Values.ContainsKey("FolderPath"))
                    return (string) _config.Values["FolderPath"];
                return String.Empty;
            }
            set { _config.Values["FolderPath"] = value; }
        }

        public static string UserName
        {
            get
            {
                if (_config.Values.ContainsKey("UserName"))
                    return (string)_config.Values["UserName"];
                return String.Empty;
            }
            set { _config.Values["UserName"] = value; }
        }

        public static string Password
        {
            get
            {
                if (_config.Values.ContainsKey("Password"))
                    return (string)_config.Values["Password"];
                return String.Empty;
            }
            set { _config.Values["Password"] = value; }
        }

        public static string LastSync
        {
            get
            {
                if (_config.Values.ContainsKey("LastSync"))
                    return (string)_config.Values["LastSync"];
                return DateTime.MinValue.ToString("yyyy\\-MM\\-dd\\THH\\:mm\\:ss\\Z");
            }
            set { _config.Values["LastSync"] = value; }
        }

        public static long MaxDownloadSize
        {
            get
            {
                if (_config.Values.ContainsKey("MaxDownloadSize"))
                    return (long)_config.Values["MaxDownloadSize"];
                return 500;
            }
            set { _config.Values["MaxDownloadSize"] = value; }
        }

        public static bool CurrentlyActive
        {
            get
            {
                if (_config.Values.ContainsKey("CurrentlyActive"))
                    return (bool)_config.Values["CurrentlyActive"];
                return false;
            }
            set { _config.Values["CurrentlyActive"] = value; }
        }

        public static NetworkCredential Credential => new NetworkCredential(UserName, Password);
    }
}
