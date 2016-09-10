using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using OwncloudUniversal.Shared;
using OwncloudUniversal.Shared.SQLite;
using OwncloudUniversal.Shared.Synchronisation;

// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace OwncloudUniversal.UI
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class Settings : Page
    {
        public Settings()
        {
            this.InitializeComponent();
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += (s, a) =>
            {
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                    a.Handled = true;
                }
            };
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Configuration.ServerUrl = txtServerUrl.Text;
            Configuration.UserName = txtUsername.Text;
            Configuration.Password = pwBox.Password;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            txtServerUrl.Text = Configuration.ServerUrl;
            txtUsername.Text = Configuration.UserName;
            pwBox.Password = Configuration.Password;

        }

        private void btnMapping_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof (FolderMapping));
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            SQLiteClient.Reset();
        }

        private void btnTask_Click(object sender, RoutedEventArgs e)
        {
            BackgroundTaskConfiguguration c = new BackgroundTaskConfiguguration();
            c.Register();
        }
    }
}
