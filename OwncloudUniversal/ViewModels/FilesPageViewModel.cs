using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using Microsoft.Toolkit.Uwp.UI;
using OwncloudUniversal.Services;
using OwncloudUniversal.Shared;
using OwncloudUniversal.Shared.LocalFileSystem;
using OwncloudUniversal.Shared.Model;
using OwncloudUniversal.Views;
using OwncloudUniversal.WebDav;
using OwncloudUniversal.WebDav.Model;
using Template10.Mvvm;
using Template10.Utils;

namespace OwncloudUniversal.ViewModels
{
    public class FilesPageViewModel : ViewModelBase
    {
        private DavItem _selectedItem;
        private readonly WebDavItemService _davItemService;
        private readonly SyncedFoldersService _syncedFolderService;
        private ObservableCollection<DavItem> _itemsList;
        private ListViewSelectionMode _selectionMode = ListViewSelectionMode.Single;

        public FilesPageViewModel()
        {
            _davItemService = WebDavItemService.GetDefault();
            _syncedFolderService = new SyncedFoldersService();
            UploadItemCommand = new DelegateCommand(async () => await NavigationService.NavigateAsync(typeof(FileTransferPage), SelectedItem) );   
            RefreshCommand = new DelegateCommand(async () => await LoadItems());
            AddToSyncCommand = new DelegateCommand<object>(async parameter => await RegisterFolderForSync(parameter));
            DownloadSingleCommand = new DelegateCommand<AbstractItem>(async item => await NavigationService.NavigateAsync(typeof(FileTransferPage), new List<AbstractItem>() {item}));
            DownloadMultipleCommand = new DelegateCommand(async () => await NavigationService.NavigateAsync(typeof(FileTransferPage), FilesPage.GetSelectedItems()));
            DeleteCommand = new DelegateCommand<DavItem>(async item => await DeleteItems(new List<AbstractItem>() {item}));
            DeleteMultipleCommand = new DelegateCommand(async () => await DeleteItems(FilesPage.GetSelectedItems()));
            SwitchSelectionModeCommand = new DelegateCommand(() => SelectionMode = SelectionMode == ListViewSelectionMode.Multiple ? ListViewSelectionMode.Single : ListViewSelectionMode.Multiple);
            ShowPropertiesCommand = new DelegateCommand<DavItem>(async item => await NavigationService.NavigateAsync(typeof(DetailsPage), item));
            AddFolderCommand = new DelegateCommand(async () => await CreateFolderAsync());
        }

        public ICommand UploadItemCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }
        public ICommand AddToSyncCommand { get; private set; }
        public ICommand RemoveFromSyncCommand { get; private set; }
        public ICommand ShowPropertiesCommand { get; private set; }
        public ICommand DownloadSingleCommand { get; private set; }
        public ICommand DownloadMultipleCommand { get; private set; }
        public ICommand RenameCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand DeleteMultipleCommand { get; private set; }
        public ICommand AddFolderCommand { get; private set; }

        public ICommand SwitchSelectionModeCommand { get; private set; }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            await base.OnNavigatedToAsync(parameter, mode, state);
            if(Configuration.IsFirstRun)
                return;
            var loginTask = Task.CompletedTask;
            var item = parameter as DavItem;
            if (item == null)
            {
                loginTask = CheckLoginAsync();
                item = new DavItem();
                item.EntityId = Configuration.ServerUrl;
                item.IsCollection = true;
            }
            if (item.IsCollection)
            {
                _selectedItem = item;
                await LoadItems();
            }
            await loginTask;
        }

        private async Task CheckLoginAsync()
        {
            IndicatorService.GetDefault().ShowBar();
            var status = await OcsClient.GetServerStatusAsync(Configuration.ServerUrl);
            if (status == null)
            {
                Shell.WelcomeDialog.IsModal = true;
            }

            var ocsClient = new OcsClient(new Uri(Configuration.ServerUrl, UriKind.RelativeOrAbsolute), Configuration.Credential);
            if (await ocsClient.CheckUserLoginAsync() == HttpStatusCode.Ok)
            {
                Shell.WelcomeDialog.IsModal = false;
            }
            else
            {
                Shell.WelcomeDialog.IsModal = true;
            }
            IndicatorService.GetDefault().HideBar();
        }
        
        public ObservableCollection<DavItem> ItemsList
        {
            get { return _itemsList; }
            private set
            {
                _itemsList = value;
                RaisePropertyChanged();
            }
        }

        public DavItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value != null && value.IsCollection && SelectionMode == ListViewSelectionMode.Single)
                {
                    _selectedItem = value;
                    NavigationService.Navigate(typeof(FilesPage), value, new SuppressNavigationTransitionInfo());
                }
            } 
        }

        public ListViewSelectionMode SelectionMode
        {
            get { return _selectionMode; }
            private set
            {
                _selectionMode = value;
                RaisePropertyChanged();
            }
        }

        private async Task LoadItems()
        {
            IndicatorService.GetDefault().ShowBar();
            var items = await _davItemService.GetItemsAsync(new Uri(_selectedItem.EntityId, UriKind.RelativeOrAbsolute));
            items.RemoveAt(0);
            ItemsList = items.OrderBy(x => !x.IsCollection).ThenBy(x=> x.DisplayName, StringComparer.CurrentCultureIgnoreCase).Cast<DavItem>().ToObservableCollection();
            //LoadThumbnails();
            Dispatcher.Dispatch(LoadThumbnails);
            IndicatorService.GetDefault().HideBar();
        }

        private async Task RegisterFolderForSync(object parameter)
        {
            if (parameter is DavItem)
            {
                var folderPicker = new FolderPicker();
                folderPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                folderPicker.FileTypeFilter.Add(".");
                var folder = await folderPicker.PickSingleFolderAsync();
                if (folder == null)
                    return;
                await _syncedFolderService.AddFolderToSyncAsync(folder, (DavItem) parameter);
                await LoadItems();
            }
        }

        private async Task DeleteItems(List<AbstractItem> items)
        {
            ContentDialog dialog = new ContentDialog();
            if (items.Count == 1)
            {
                var item = items.First();
                dialog.Title = App.ResourceLoader.GetString("deleteFileConfirmation");
                if (item.IsCollection)
                    dialog.Title = App.ResourceLoader.GetString("deleteFolderConfirmation");
                dialog.Content = item.DisplayName;
            }
            else
            {
                dialog.Title = App.ResourceLoader.GetString("deleteMultipleConfirmation");
                int i = 0;
                foreach (var item in items)
                {
                    if (i < 3)
                    {
                        dialog.Content += item.DisplayName + Environment.NewLine;
                        i++;
                    }
                    else
                    {
                        dialog.Content += "...";
                        break;
                    }
                }
            }
            dialog.PrimaryButtonText = App.ResourceLoader.GetString("yes");
            dialog.SecondaryButtonText = App.ResourceLoader.GetString("no");
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                IndicatorService.GetDefault().ShowBar();
                await WebDavItemService.GetDefault().DeleteItemAsync(items.Cast<DavItem>().ToList());
                SelectionMode = ListViewSelectionMode.Single;
                await LoadItems();
                IndicatorService.GetDefault().HideBar();
            }
            
        }

        private async Task CreateFolderAsync()
        {
            var dialog = new ContentDialog();
            var box = new TextBox()
            {
                Header = "Name"
            };
            dialog.Content = box;
            dialog.PrimaryButtonText = "OK";
            dialog.SecondaryButtonText = "Cancel";
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                IndicatorService.GetDefault().ShowBar();
                await WebDavItemService.GetDefault().CreateFolder(SelectedItem, box.Text);
                await LoadItems();
                IndicatorService.GetDefault().HideBar();
            }
        }

        private void LoadThumbnails()
        {
            foreach (var davItem in ItemsList)
            {
                if (!davItem.IsCollection && davItem.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
                {
                    var serverUrl = Configuration.ServerUrl.Substring(0, Configuration.ServerUrl.IndexOf("remote.php", StringComparison.OrdinalIgnoreCase));
                    var itemPath = davItem.EntityId.Substring(davItem.EntityId.IndexOf("remote.php/webdav", StringComparison.OrdinalIgnoreCase) + 17);
                    var url = serverUrl + "index.php/apps/files/api/v1/thumbnail/" + 40 + "/" + 40 + itemPath;
                    ImageCache.Instance.PreCacheAsync(new Uri(url), false, true);// no need to await this
                    davItem.ThumbnailUrl = url;
                }
            }
        }
    }
}
