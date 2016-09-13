using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.Background;
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
using OwncloudUniversal.Shared.WebDav;
using Windows.System;

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

        private Folder _currentFolder;

        private void BackRequestet(object sender, BackRequestedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
                return;
            if (rootFrame.CanGoBack && args.Handled == false)
            {
                args.Handled = true;
                rootFrame.GoBack();
            }
            SystemNavigationManager.GetForCurrentView().BackRequested -= BackRequestet;
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

        private async void CreateListView(Folder folder)
        {
            List<RemoteItem> list = new List<RemoteItem>();
            try
            {
               list = await folder.LoadItems();
            }
            catch (Exception e)
            {
                MessageDialog d = new MessageDialog("Failed to load items. Please check connectivity and configuration. " + e.Message);
                await d.ShowAsync();
                return;
            }
            var orderedList = list.OrderBy(x => !x.DavItem.IsCollection).ThenBy(x => x.DavItem.DisplayName);
            listView.ItemsSource = orderedList;
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
                    await Launcher.LaunchFileAsync(file);
                }
            }

        }

        private async void appBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            progressBar.IsIndeterminate = true;
            progressBar.Visibility = Visibility.Visible;
            ProcessingManager s = new ProcessingManager(new FileSystemAdapter(), new WebDavAdapter());
            await s.Run();
            progressBar.IsIndeterminate = false;
            progressBar.Visibility = Visibility.Collapsed;
            MessageDialog d = new MessageDialog("Scan Finished.");
            await d.ShowAsync();

        }

        private async void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            //what the hell, no wild cards allowed??????????
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".odt");
            picker.FileTypeFilter.Add(".ods");
            picker.FileTypeFilter.Add(".odp");
            picker.FileTypeFilter.Add(".doc");
            picker.FileTypeFilter.Add(".docx");
            picker.FileTypeFilter.Add(".mp4");
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".zip");
            picker.FileTypeFilter.Add(".rar");
            picker.FileTypeFilter.Add(".txt");
            picker.FileTypeFilter.Add(".pdf");
            picker.FileTypeFilter.Add(".odf");
            picker.FileTypeFilter.Add(".kdbx");
            picker.FileTypeFilter.Add(".ppt");
            picker.FileTypeFilter.Add(".pptx");
            picker.FileTypeFilter.Add(".xlsx");
            picker.FileTypeFilter.Add(".xls");
            picker.FileTypeFilter.Add(".gif");
            //picker.FileTypeFilter.Add(".tar.gz");
            picker.FileTypeFilter.Add(".mid");
            picker.FileTypeFilter.Add(".aac");
            var files = await picker.PickMultipleFilesAsync();
            if (files.Count > 0)
            {
                foreach (var file in files)
                {
                    StorageApplicationPermissions.FutureAccessList.AddOrReplace("uploadFile", file);
                    using (var stream = await file.OpenStreamForReadAsync())
                    {
                        if (string.IsNullOrWhiteSpace(_currentFolder.Href))
                        {
                            await ConnectionManager.Upload(Configuration.ServerUrl, stream, file.Name);
                        }
                        else
                        {
                            await ConnectionManager.Upload(_currentFolder.Href, stream, file.Name);
                        }
                    }
                    
                }
            }
            MessageDialog dialog = new MessageDialog("Upload Finished");
            await dialog.ShowAsync();
            CreateListView(_currentFolder);
        }
    }
}
