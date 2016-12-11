using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Popups;
using OwncloudUniversal.Services;
using OwncloudUniversal.Shared;
using OwncloudUniversal.Views;
using OwncloudUniversal.WebDav;
using Template10.Mvvm;

namespace OwncloudUniversal.ViewModels
{
    public class WelcomePageViewModel : ViewModelBase
    {
        private bool _serverFound;

        public bool ServerFound
        {
            get { return _serverFound; }
            private set
            {
                _serverFound = value;
                RaisePropertyChanged();
            }  
        }

        public string ServerUrl
        {
            get { return Configuration.ServerUrl; }
            set
            {
                Configuration.ServerUrl = value;
#pragma warning disable 4014
                CheckServerStatus();
#pragma warning restore 4014
            }
        }

        public string UserName
        {
            get { return Configuration.UserName; }
            set { Configuration.UserName = value; }
        }

        public string Password
        {
            get { return Configuration.Password; }
            set { Configuration.Password = value; }
        }

        public ICommand ConnectCommand { get; private set; }

        public WelcomePageViewModel()
        {
            ServerFound = false;
#pragma warning disable 4014
            CheckServerStatus();
#pragma warning restore 4014
            ConnectCommand = new DelegateCommand(async () => await Connect());
        }

        private async Task CheckServerStatus()
        {
            var status = await OcsClient.GetServerStatusAsync(ServerUrl);
            if (status != null && status.Installed && !status.Maintenance)
                ServerFound = true;
            else
                ServerFound = false;
        }

        private async Task Connect()
        {
            await CheckServerStatus();
            if (ServerFound)
            {
                IndicatorService.GetDefault().ShowBar();
                OcsClient client = new OcsClient(new Uri(ServerUrl),new NetworkCredential(UserName, Password));
                var success = await client.CheckUserLoginAsync();
                if (success)
                {

                    NavigationService.Navigate(typeof(FilesPage));
                }
                else
                {
                    MessageDialog dialog = new MessageDialog("Login Failed");
                    await dialog.ShowAsync();
                }
                IndicatorService.GetDefault().HideBar();
            }
        }
}
}
