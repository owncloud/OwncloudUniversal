using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using owncloud_universal.Model;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
//using owncloud_universal.WebDav;


// Die Vorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 dokumentiert.

namespace owncloud_universal
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.RequestedTheme = ElementTheme.Dark;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += BackRequestet;
        }

        private Folder _currentFolder;

        private void BackRequestet(object sender, BackRequestedEventArgs args)
        {
            if (!Frame.CanGoBack || args.Handled != false) return;
            args.Handled = true;
            Frame.GoBack();
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            CreateListView(_currentFolder);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!ConnectionManager.IsSetup)
                if (!ConnectionManager.SetUp())
                    return;

            string path = Configuration.FolderPath;
            if (e.Parameter is RemoteItem)
                path = (e.Parameter as RemoteItem).DavItem.Href;
            Folder folder = new Folder(path);
            _currentFolder = folder;
            CreateListView(folder);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var frame = Window.Current.Content as Frame;
            frame.Navigate(typeof(Settings));
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = e.AddedItems[0] as RemoteItem;
            if (!item.DavItem.IsCollection)
            {
                Download(item);
                return;
            }
            var frame = Window.Current.Content as Frame;
            frame.Navigate(typeof(MainPage), item);
        }

        private async void listView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            MessageDialog dia = new MessageDialog("Sync this item?");
            dia.Commands.Add(new UICommand("yes", null, "YES"));
            dia.Commands.Add(new UICommand("no", null, "NO"));
            var result = await dia.ShowAsync();
            if (result.Id.ToString() == "YES")
            {
                if(e.OriginalSource is TextBlock)
                    AddToSync(((TextBlock)e.OriginalSource).DataContext as RemoteItem);
                if(e.OriginalSource is ListViewItemPresenter)
                    AddToSync(((ListViewItemPresenter)e.OriginalSource).DataContext as RemoteItem);
            }
        }

        private async void AddToSync(RemoteItem item)
        {
            var folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            folderPicker.FileTypeFilter.Add(".");
            var folder = await folderPicker.PickSingleFolderAsync();
            if(folder == null)
                return;

            StorageApplicationPermissions.FutureAccessList.Add(folder);
            var properties = await folder.Properties.RetrievePropertiesAsync(new List<string> { "System.DateModified" });
            LocalItem li = new LocalItem
            {
                IsCollection = true,
                LastModified = ((DateTimeOffset)properties["System.DateModified"]).LocalDateTime,
                Path = folder.Path
            };
            LocalItemTableModel.GetDefault().InsertItem(li);
            li = LocalItemTableModel.GetDefault().GetLastInsertItem();

            RemoteItemTableModel.GetDefault().InsertItem(item);
            item = RemoteItemTableModel.GetDefault().GetLastInsertItem();

            FolderAssociation fa = new FolderAssociation
            {
                IsActive = true,
                LocalItem = li,
                RemoteItem = item,
                SyncDirection = SyncDirection.TwoWay
            };
            FolderAssociationTableModel.GetDefault().InsertItem(fa);
            fa = FolderAssociationTableModel.GetDefault().GetLastInsertItem();
        }

        private async void CreateListView(Folder folder)
        {
            var list = await folder.LoadItems();
            listView.ItemsSource = list;
        }

        private async void Download(RemoteItem item)
        {
            MessageDialog dia = new MessageDialog("Download?");
            dia.Commands.Add(new UICommand("yes", null, "YES"));
            dia.Commands.Add(new UICommand("no", null, "NO"));
            var result = await dia.ShowAsync();
            if (result.Id.ToString() == "YES")
            {
                var savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                string extension = Path.GetExtension(item.DavItem.DisplayName);
                if (String.IsNullOrWhiteSpace(extension))
                    extension = ".txt";
                savePicker.FileTypeChoices.Add("All Files", new List<string>() { extension });
                savePicker.SuggestedFileName = item.DavItem.DisplayName;
                var file = await savePicker.PickSaveFileAsync();
                
                var success = await ConnectionManager.Download(item.DavItem.Href, file);
                if (success)
                {
                    MessageDialog d = new MessageDialog("Download Finished.");
                    await d.ShowAsync();
                }
            }

        }

        private void appBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            SyncWorker s = new SyncWorker();
            s.Run();
        }
    }
}
