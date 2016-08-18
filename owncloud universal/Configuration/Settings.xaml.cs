using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace owncloud_universal
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
    }
}
