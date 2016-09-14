using System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using OwncloudUniversal.Shared;
using OwncloudUniversal.Shared.SQLite;
using OwncloudUniversal.Shared.Synchronisation;
using OwncloudUniversal.Shared.WebDav;

// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace OwncloudUniversal.UI
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class Settings : Page
    {

        BackgroundTaskConfiguration taskConfig = new BackgroundTaskConfiguration();
        public Settings()
        {
            this.InitializeComponent();
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += BackRequestet;
        }

        private void BackRequestet(object sender, BackRequestedEventArgs args)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested -= BackRequestet;
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
                return;
            if (rootFrame.CanGoBack && args.Handled == false)
            {
                args.Handled = true;
                rootFrame.GoBack();
            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Configuration.ServerUrl = txtServerUrl.Text;
            Configuration.UserName = txtUsername.Text;
            Configuration.Password = pwBox.Password;
            ConnectionManager.SetUp();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            txtServerUrl.Text = Configuration.ServerUrl;
            txtUsername.Text = Configuration.UserName;
            pwBox.Password = Configuration.Password;
            toggleSwitch.IsOn = taskConfig.Enabled;

        }

        private async void btnReset_Click(object sender, RoutedEventArgs e)
        {
            SQLiteClient.Reset();
            MessageDialog dialog = new MessageDialog("Database reset. Please configure the synced folders again.");
            await dialog.ShowAsync();
        }

        private void toggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            taskConfig.Enabled = toggleSwitch.IsOn;
        }

        private void btnFolderPairs_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(FolderMapping));
        }
    }
}
