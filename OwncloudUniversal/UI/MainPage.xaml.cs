using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using OwncloudUniversal;
using OwncloudUniversal.Shared;
using OwncloudUniversal.Shared.LocalFileSystem;
using OwncloudUniversal.Shared.Model;
using OwncloudUniversal.Shared.Synchronisation;
using Windows.System;
using Windows.System.Display;
using OwncloudUniversal.WebDav;

//using owncloud_universal.WebDav;


// Die Vorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 dokumentiert.

namespace OwncloudUniversal.UI
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += BackRequestet;
        }

        private DavFolder _currentFolder;
        public List<DavItem> FolderContent { get; set; }

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
            //just trigger an update ;)
            _currentFolder.Items = _currentFolder.Items;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            AbstractItem parameterItem = null;
            if (e.Parameter is DavItem)
                parameterItem = (e.Parameter as AbstractItem);
            if(parameterItem == null)
                parameterItem = new AbstractItem { EntityId = Configuration.ServerUrl};
            _currentFolder = new DavFolder {Href = new Uri(parameterItem.EntityId, UriKind.RelativeOrAbsolute)};
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Settings));
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = e.AddedItems[0] as AbstractItem;
            if (!item.IsCollection)
            {
                Download(item);
                return;
            }
            Frame.Navigate(typeof(MainPage), item);
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
                    AddToSync(((TextBlock)e.OriginalSource).DataContext as AbstractItem);
                if(e.OriginalSource is ListViewItemPresenter)
                    AddToSync(((ListViewItemPresenter)e.OriginalSource).DataContext as AbstractItem);
            }
        }

        private async void AddToSync(AbstractItem item)
        {
            var folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            folderPicker.FileTypeFilter.Add(".");
            var folder = await folderPicker.PickSingleFolderAsync();
            if(folder == null)
                return;

            StorageApplicationPermissions.FutureAccessList.Add(folder);
            var properties = await folder.Properties.RetrievePropertiesAsync(new List<string> { "System.DateModified" });

            FolderAssociation fa = new FolderAssociation
            {
                IsActive = true,
                LocalFolderId = 0,
                RemoteFolderId = 0,
                SyncDirection = SyncDirection.TwoWay
            };
            FolderAssociationTableModel.GetDefault().InsertItem(fa);
            fa = FolderAssociationTableModel.GetDefault().GetLastInsertItem();

            AbstractItem li = new LocalItem
            {
                IsCollection = true,
                LastModified = ((DateTimeOffset)properties["System.DateModified"]).LocalDateTime,
                EntityId = folder.Path,
                Association = fa,
            };
            AbstractItemTableModel.GetDefault().InsertItem(li);
            li = AbstractItemTableModel.GetDefault().GetLastInsertItem();

            item.Association = fa;
            AbstractItemTableModel.GetDefault().InsertItem(item);
            var ri = AbstractItemTableModel.GetDefault().GetLastInsertItem();

            fa.RemoteFolderId = ri.Id;
            fa.LocalFolderId = li.Id;
            FolderAssociationTableModel.GetDefault().UpdateItem(fa, fa.Id);
        }

        private void Download(AbstractItem item)
        {
            throw new NotImplementedException();
        }
        private void appBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SyncMonitor));
        }

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
