using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using OwncloudUniversal.Services;
using OwncloudUniversal.Shared;
using OwncloudUniversal.Views;
using OwncloudUniversal.WebDav;
using OwncloudUniversal.WebDav.Model;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using HttpStatusCode = Windows.Web.Http.HttpStatusCode;

namespace OwncloudUniversal.ViewModels
{
    public class WelcomePageViewModel : ViewModelBase
    {
        private bool _serverFound;
        private string _responseCode;
        private string _serverUrl;
        private string _password;
        private string _userName;

        public string ServerUrl
        {
            get
            {
                _serverUrl = Configuration.ServerUrl;
                return _serverUrl;
            }
            set
            {
                _serverUrl = value;
                var task = CheckServerStatus();
            }
        }

        public string UserName
        {
            get
            {
                _userName = Configuration.UserName;
                return _userName;
            }
            set { _userName = value; }
        }

        public string Password
        {
            get
            {
                _password = Configuration.Password;
                return _password;
            }
            set { _password = value; }
        }

        public string ResponseCode
        {
            get { return _responseCode; }
            set
            {
                _responseCode = value;
                RaisePropertyChanged();
            }
        }

        public ICommand ConnectCommand { get; private set; }

        public WelcomePageViewModel()
        {
            ConnectCommand = new DelegateCommand(async () => await Connect());
        }

        private async Task CheckServerStatus()
        {
            ServerStatus status = null;
            try
            {
                status = await OcsClient.GetServerStatusAsync(_serverUrl);
                ResponseCode = status?.ResponseCode;
            }
            catch (Exception e)
            {
                if ((uint) e.HResult == 0x80072EE7)
                    ResponseCode = App.ResourceLoader.GetString("ServerNotFound");
                else throw;
            }
            if (status != null && status.Installed && !status.Maintenance)
                _serverFound = true;
            else
                _serverFound = false;
        }

        private async Task Connect()
        {
            IndicatorService.GetDefault().ShowBar();
            try
            {
                await CheckServerStatus();
                if (_serverFound)
                {

                    OcsClient client = new OcsClient(new Uri(_serverUrl), new NetworkCredential(_userName, _password));
                    var status = await client.CheckUserLoginAsync();
                    if (status == HttpStatusCode.Ok)
                    {
                        Configuration.ServerUrl = OcsClient.GetWebDavUrl(_serverUrl);
                        Configuration.Password = _password;
                        Configuration.UserName = _userName;
                        Configuration.IsFirstRun = false;
                        Shell.WelcomeDialog.IsModal = false;
                        await NavigationService.NavigateAsync(typeof(FilesPage));
                    }
                    else
                    {
                        MessageDialog dialog = new MessageDialog(status.ToString());
                        await dialog.ShowAsync();
                    }
                }
            }
            catch (Exception e)
            {
                if ((uint)e.HResult == 0x80072EE7)
                    ResponseCode = App.ResourceLoader.GetString("ServerNotFound");
                else throw;
            }
            IndicatorService.GetDefault().HideBar();
        }
    }
}
