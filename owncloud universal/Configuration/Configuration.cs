using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace owncloud_universal
{
    public static class Configuration
    {
        private static Windows.Storage.ApplicationDataContainer Config = Windows.Storage.ApplicationData.Current.LocalSettings;

        public static string ServerName
        {
            get
            {
                if (Config.Values.ContainsKey("ServerName"))
                    return (string)Config.Values["ServerName"];
                return String.Empty;
            }
            set { Config.Values["ServerName"] = value; } 
        }

        public static string FolderPath
        {
            get
            {
                if (Config.Values.ContainsKey("FolderPath"))
                    return (string) Config.Values["FolderPath"];
                return String.Empty;
            }
            set { Config.Values["FolderPath"] = value; }
        }

        public static string UserName
        {
            get
            {
                if (Config.Values.ContainsKey("UserName"))
                    return (string)Config.Values["UserName"];
                return String.Empty;
            }
            set { Config.Values["UserName"] = value; }
        }

        public static string Password
        {
            get
            {
                if (Config.Values.ContainsKey("Password"))
                    return (string)Config.Values["Password"];
                return String.Empty;
            }
            set { Config.Values["Password"] = value; }
        }
    }
}
