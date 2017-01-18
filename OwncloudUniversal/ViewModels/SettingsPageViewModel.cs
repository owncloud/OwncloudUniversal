using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Template10.Mvvm;
using Template10.Services.SettingsService;
using Windows.UI.Xaml;
using OwncloudUniversal.Shared;
using OwncloudUniversal.Shared.SQLite;
using OwncloudUniversal.Shared.Synchronisation;
using SettingsService = OwncloudUniversal.Services.SettingsServices.SettingsService;

namespace OwncloudUniversal.ViewModels
{
    public class SettingsPageViewModel : ViewModelBase
    {
        public SettingsPartViewModel SettingsPartViewModel { get; } = new SettingsPartViewModel();
        public AboutPartViewModel AboutPartViewModel { get; } = new AboutPartViewModel();
    }

    public class SettingsPartViewModel : ViewModelBase
    {
        Services.SettingsServices.SettingsService _settings;
        BackgroundTaskConfiguration _taskConfig = new BackgroundTaskConfiguration();

        public ICommand ResetCommand { get; }

        public SettingsPartViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                // designtime
            }
            else
            {
                _settings = Services.SettingsServices.SettingsService.Instance;
            }

            ResetCommand = new DelegateCommand(async () => await ResetDataBaseAsync());
        }

        public bool ShowHamburgerButton
        {
            get { return _settings.ShowHamburgerButton; }
            set { _settings.ShowHamburgerButton = value; base.RaisePropertyChanged(); }
        }

        public bool IsFullScreen
        {
            get { return _settings.IsFullScreen; }
            set
            {
                _settings.IsFullScreen = value;
                base.RaisePropertyChanged();
                if (value)
                {
                    ShowHamburgerButton = false;
                }
                else
                {
                    ShowHamburgerButton = true;
                }
            }
        }

        public bool UseShellBackButton
        {
            get { return _settings.UseShellBackButton; }
            set { _settings.UseShellBackButton = value; base.RaisePropertyChanged(); }
        }

        public bool UseLightThemeButton
        {
            get { return _settings.AppTheme.Equals(ApplicationTheme.Light); }
            set
            {
                _settings.AppTheme = value ? ApplicationTheme.Light : ApplicationTheme.Dark; base.RaisePropertyChanged();
                if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
                {
                    var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                    if (titleBar != null)
                    {
                        if (Services.SettingsServices.SettingsService.Instance.AppTheme == ApplicationTheme.Dark)
                        {
                            titleBar.BackgroundColor = Colors.Black;
                            titleBar.ForegroundColor = Colors.White;
                        }
                        else
                        {
                            titleBar.BackgroundColor = Colors.White;
                            titleBar.ForegroundColor = Colors.Black;
                        }
                    }
                }

                if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                {

                    var statusBar = StatusBar.GetForCurrentView();
                    if (statusBar != null)
                    {
                        if (SettingsService.Instance.AppTheme == ApplicationTheme.Dark)
                        {
                            statusBar.BackgroundColor = Colors.Black;
                            statusBar.ForegroundColor = Colors.White;
                        }
                        else
                        {
                            statusBar.BackgroundColor = Colors.White;
                            statusBar.ForegroundColor = Colors.Black;
                        }
                    }
                }
            }
        }

        public string ServerUrl
        {
            get { return Configuration.ServerUrl; }
            set { Configuration.ServerUrl = value; }
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

        private async Task ResetDataBaseAsync()
        {
            MessageDialog areYouSure = new MessageDialog("Do you really want to reset the Synchronization-Database?");
            areYouSure.Commands.Add(new UICommand("Yes", null, "YES"));
            areYouSure.Commands.Add(new UICommand("No", null, "NO"));
            var result = await areYouSure.ShowAsync();
            if (result.Id.ToString() == "YES")
            {
                SQLiteClient.Reset();
                Configuration.LastSync = DateTime.MinValue.ToString("yyyy\\-MM\\-dd\\THH\\:mm\\:ss\\Z");
                LogHelper.ResetLog();
                MessageDialog dialog = new MessageDialog("Database has been reset. Please reconfigure your synced folders.");
                await dialog.ShowAsync();
            }
        }

        public bool BackgroundTaskEnabled
        {
            get { return _taskConfig.Enabled; }
            set { _taskConfig.Enabled = value; }
        }
    }

    public class AboutPartViewModel : ViewModelBase
    {
        public Uri Logo => Windows.ApplicationModel.Package.Current.Logo;

        public string DisplayName => Windows.ApplicationModel.Package.Current.DisplayName;

        public string Publisher => Windows.ApplicationModel.Package.Current.PublisherDisplayName;

        public string Version
        {
            get
            {
                var v = Windows.ApplicationModel.Package.Current.Id.Version;
                return $"{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";
            }
        }

        public Uri RateMe => new Uri("http://aka.ms/template10");
    }
}

