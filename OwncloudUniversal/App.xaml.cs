using Windows.UI.Xaml;
using System.Threading.Tasks;
using OwncloudUniversal.Services.SettingsServices;
using Windows.ApplicationModel.Activation;
using Template10.Controls;
using Template10.Common;
using System;
using System.Linq;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Controls;
using OwncloudUniversal.Shared;
using OwncloudUniversal.Shared.SQLite;
using OwncloudUniversal.Views;
using OwncloudUniversal.WebDav;

namespace OwncloudUniversal
{
    /// Documentation on APIs used in this page:
    /// https://github.com/Windows-XAML/Template10/wiki

    [Bindable]
    sealed partial class App : BootStrapper
    {
        public App()
        {
            InitializeComponent();
            SplashFactory = (e) => new Views.Splash(e);

            #region app settings

            // some settings must be set in app.constructor
            var settings = SettingsService.Instance;
            RequestedTheme = settings.AppTheme;
            CacheMaxDuration = settings.CacheMaxDuration;
            ShowShellBackButton = settings.UseShellBackButton;
            AutoSuspendAllFrames = true;
            AutoRestoreAfterTerminated = true;
            AutoExtendExecutionSession = true;

            #endregion
        }

        public static readonly ResourceLoader ResourceLoader = new ResourceLoader();

        public override UIElement CreateRootElement(IActivatedEventArgs e)
        {
            SetTitleTheme();
            var service = NavigationServiceFactory(BackButton.Attach, ExistingContent.Include);
            return new ModalDialog
            {
                DisableBackButtonWhenModal = true,
                Content = new Views.Shell(service),
            };
        }

        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            // TODO: add your long-running task here
            if (startKind == StartKind.Launch)
            {
                SQLiteClient.Init();
                var status = await OcsClient.GetServerStatusAsync(Configuration.ServerUrl);
                if (status == null)
                    await NavigationService.NavigateAsync(typeof(WelcomePage));
                else
                {
                    await NavigationService.NavigateAsync(typeof(FilesPage));
                }
            }
        }

        private void SetTitleTheme()
        {
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
            {
                var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                if (titleBar != null)
                {
                    if (SettingsService.Instance.AppTheme == ApplicationTheme.Dark)
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
}

