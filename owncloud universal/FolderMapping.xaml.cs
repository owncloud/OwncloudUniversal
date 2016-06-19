using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using owncloud_universal.Model;
using System.ComponentModel;

// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace owncloud_universal
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class FolderMapping : Page
    {
        public FolderMapping()
        {
            this.InitializeComponent();
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += (s, a) =>
            {
                if (Frame.CanGoBack)
                {
                    Frame.Navigate(typeof(Settings));
                    a.Handled = true;
                }
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            LoadSyncItems();
        }

        private void LoadSyncItems()
        {
            var associations = FolderAssociationTableModel.GetDefault().GetAllItems();           
            listView.ItemsSource = associations;
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var fa = listView.SelectedItem as FolderAssociation;
            var model = FolderAssociationTableModel.GetDefault();
            model.DeleteItem(fa.Id);
            LoadSyncItems();
            //how can i update those fucking items
            //Frame.Navigate(typeof (FolderMapping));
        }
    }
}
