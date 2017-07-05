using System;
using System.Linq;
using System.Net;
using Windows.Security.Credentials;
using Windows.Storage;

namespace OwncloudUniversal.Synchronization.Configuration
{
    public static class Configuration
    {
        private static readonly Windows.Storage.ApplicationDataContainer Config = ApplicationData.Current.LocalSettings.CreateContainer(ContainerName, ApplicationDataCreateDisposition.Always);

        private static string _password = "";
        private static string _username = "";
        private static string _serverUrl = "";
        private const string ContainerName = "ownCloud";

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
        

        public static long MaxDownloadSize
        {
            get
            {
                if (Config.Values.ContainsKey("MaxDownloadSize"))
                    return (long)Config.Values["MaxDownloadSize"];
                return 500;
            }
            set { Config.Values["MaxDownloadSize"] = value; }
        }

        public static bool IsFirstRun
        {
            get
            {
                if (Config.Values.ContainsKey("IsFirstRun"))
                    return (bool)Config.Values["IsFirstRun"];
                return true;
            }
            set { Config.Values["IsFirstRun"] = value; }
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

        /// <summary>
        /// Indicates wether the background task is enabled so it can be registered again after an app update
        /// </summary>
        public static bool IsBackgroundTaskEnabled
        {
            get
            {
                if (Config.Values.ContainsKey("IsBackgroundTaskEnabled"))
                    return (bool)Config.Values["IsBackgroundTaskEnabled"];
                return true;
            }
            set { Config.Values["IsBackgroundTaskEnabled"] = value; }
        }

        public static bool HideDesktopClientInfo
        {
            get
            {
                if (Config.Values.ContainsKey("HideDesktopClientInfo"))
                    return (bool)Config.Values["HideDesktopClientInfo"];
                return false;
            }
            set => Config.Values["HideDesktopClientInfo"] = value;
        }

        public static bool NeedsInitialSchemaVersion
        {
            get
            {
                if (Config.Values.ContainsKey("NeedsInitialSchemaVersion"))
                    return (bool)Config.Values["NeedsInitialSchemaVersion"];
                return true;
            }
            set => Config.Values["NeedsInitialSchemaVersion"] = value;
        }

        public static void Reset()
        {
            ApplicationData.Current.LocalSettings.DeleteContainer(ContainerName);
        }
    }
}
