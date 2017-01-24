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
        private static string _serverUrl = "";

        public static string ServerUrl
        {
            get
            {
                return GetCredentialFromLocker()?.Resource ?? String.Empty;
            }
            set
            {
                _serverUrl = value;
                AddCredentialToLocker();
            } 
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

        public static bool IsFirstRun
        {
            get
            {
                if (_config.Values.ContainsKey("IsFirstRun"))
                    return (bool)_config.Values["IsFirstRun"];
                return true;
            }
            set { _config.Values["IsFirstRun"] = value; }
        }

        public static NetworkCredential Credential => new NetworkCredential(UserName, Password);

        private static PasswordCredential GetCredentialFromLocker()
        {
            PasswordCredential credential = null;

            var vault = new PasswordVault();
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
            if (!(string.IsNullOrWhiteSpace(_username) || string.IsNullOrWhiteSpace(_password) || string.IsNullOrWhiteSpace(_serverUrl) ))
            {
                RemoveCredentials();
                var credential = new PasswordCredential(_serverUrl, _username, _password);
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
