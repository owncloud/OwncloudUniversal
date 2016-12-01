using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.Storage;

namespace OwncloudUniversal.Shared
{
    public static class Configuration
    {
        private static Windows.Storage.ApplicationDataContainer _config = Windows.Storage.ApplicationData.Current.LocalSettings;

        private static string _password = "";
        private static string _username = "";

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
            get { return GetCredentialFromLocker()?.UserName ?? String.Empty; }
            set
            {
                _username = value; 
                AddCredentialToLocker();
            }
        }

        public static string Password
        {
            get { return GetCredentialFromLocker()?.Password ?? String.Empty; }
            set
            {
                _password = value;
                AddCredentialToLocker();
            }
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
        public static NetworkCredential Credential => new NetworkCredential(UserName, Password);

        private static PasswordCredential GetCredentialFromLocker()
        {
            Windows.Security.Credentials.PasswordCredential credential = null;

            var vault = new Windows.Security.Credentials.PasswordVault();
            var credentialList = vault.RetrieveAll();
            if (credentialList.Count > 0)
            {
                if (credentialList.Count == 1)
                {
                    credential = credentialList[0];
                }
                else
                {
                    credential = vault.RetrieveAll().FirstOrDefault();
                }
                credential.RetrievePassword();
            }

            return credential;
        }

        private static void AddCredentialToLocker()
        {
            if (!(string.IsNullOrWhiteSpace(_username) || string.IsNullOrWhiteSpace(_password)))
            {
                var credential = new PasswordCredential("OwncloudUniversal", _username, _password);
                var vault = new Windows.Security.Credentials.PasswordVault();
                vault.Add(credential);
            }
        }

        public static void RemoveCredentials()
        {
            if(GetCredentialFromLocker() == null) return;
            var vault = new PasswordVault();
            vault.Remove(GetCredentialFromLocker());
        }
    }
}
