using Windows.UI.Xaml;
using System.Threading.Tasks;
using OwncloudUniversal.Services.SettingsServices;
using Windows.ApplicationModel.Activation;
using Template10.Controls;
using Template10.Common;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;
using OwncloudUniversal.Shared;
using OwncloudUniversal.Shared.SQLite;
using OwncloudUniversal.Utils;
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
            ThemeHelper.UpdateTitleBar();
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
    }
}

