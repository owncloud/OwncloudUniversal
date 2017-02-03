using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using Template10.Services.NavigationService;
using Template10.Utils;

namespace OwncloudUniversal.ViewModels
{
    public class FilesPageViewModel : ViewModelBase
    {
        private DavItem _selectedItem;
        private readonly SyncedFoldersService _syncedFolderService;
        private ListViewSelectionMode _selectionMode = ListViewSelectionMode.Single;

        public FilesPageViewModel()
        {
            _syncedFolderService = new SyncedFoldersService();
            WebDavNavigationService = WebDavNavigationService.GetDefault();
            UploadItemCommand = new DelegateCommand(async () => await NavigationService.NavigateAsync(typeof(FileTransferPage), WebDavNavigationService.CurrentItem) );   
            RefreshCommand = new DelegateCommand(async () => await WebDavNavigationService.ReloadAsync());
            AddToSyncCommand = new DelegateCommand<object>(async parameter => await RegisterFolderForSync(parameter));
            DownloadSingleCommand = new DelegateCommand<BaseItem>(async item => await NavigationService.NavigateAsync(typeof(FileTransferPage), new List<BaseItem>() {item}));
            DownloadMultipleCommand = new DelegateCommand(async () => await NavigationService.NavigateAsync(typeof(FileTransferPage), FilesPage.GetSelectedItems()));
            DeleteCommand = new DelegateCommand<DavItem>(async item => await DeleteItems(new List<BaseItem>() {item}));
            DeleteMultipleCommand = new DelegateCommand(async () => await DeleteItems(FilesPage.GetSelectedItems()));
            SwitchSelectionModeCommand = new DelegateCommand(() => SelectionMode = SelectionMode == ListViewSelectionMode.Multiple ? ListViewSelectionMode.Single : ListViewSelectionMode.Multiple);
            ShowPropertiesCommand = new DelegateCommand<DavItem>(async item => await NavigationService.NavigateAsync(typeof(DetailsPage), item));
            AddFolderCommand = new DelegateCommand(async () => await CreateFolderAsync());
            WebDavNavigationService.PropertyChanged += WebDavNavigationServiceOnPropertyChanged;
            HomeCommand = new DelegateCommand(async () => await WebDavNavigationService.NavigateAsync(WebDavNavigationService.BackStack[0]));
        }

        private void WebDavNavigationServiceOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "Items")
            {
                Task.Run(() => LoadThumbnails());
            }
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

        public ICommand HomeCommand { get; private set; }

        public WebDavNavigationService WebDavNavigationService { get; }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            await base.OnNavigatedToAsync(parameter, mode, state);
            WebDavNavigationService.SetNavigationService(NavigationService);
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            await base.OnNavigatingFromAsync(args);
            if (args.NavigationMode == NavigationMode.Back)
                await WebDavNavigationService.GoBackAsync();
            if (args.NavigationMode == NavigationMode.Forward && args.TargetPageParameter is DavItem)
                await WebDavNavigationService.GoForwardAsync();
            if (!(args.TargetPageType == typeof(FilesPage) || args.TargetPageType == typeof(FileTransferPage) || args.TargetPageType == typeof(DetailsPage)))
                await WebDavNavigationService.Reset();//TODO find a way to restore old navigationhistory for this frame only
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

        public DavItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if(_selectedItem == value)
                    return;
                if (value != null && SelectionMode == ListViewSelectionMode.Single)
                {
                    _selectedItem = value;
                    RaisePropertyChanged();
                    if (value.IsCollection)
                    {
                        NavigationService.Navigate(typeof(FilesPage), value, new SuppressNavigationTransitionInfo());
                        var task = WebDavNavigationService.NavigateAsync(value);
                    }
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
            }
        }

        private async Task DeleteItems(List<BaseItem> items)
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
                await WebDavNavigationService.ReloadAsync();
                IndicatorService.GetDefault().HideBar();
            }
            
        }

        private async Task CreateFolderAsync()
        {
            var dialog = new ContentDialog();
            dialog.Title = App.ResourceLoader.GetString("CreateNewFolder");
            var box = new TextBox()
            {
                Header = App.ResourceLoader.GetString("FolderName"),
                AcceptsReturn = false,
                SelectedText = App.ResourceLoader.GetString("NewFolderName")
            };
            dialog.Content = box;
            dialog.PrimaryButtonText = App.ResourceLoader.GetString("OK");
            dialog.SecondaryButtonText = App.ResourceLoader.GetString("Cancel");
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                IndicatorService.GetDefault().ShowBar();
                await WebDavItemService.GetDefault().CreateFolder(WebDavNavigationService.CurrentItem, box.Text);
                await WebDavNavigationService.ReloadAsync();
                IndicatorService.GetDefault().HideBar();
            }
        }

        private void LoadThumbnails()
        {
            if(WebDavNavigationService.Items == null)
                return;

            var serverUrl = Configuration.ServerUrl.Substring(0, Configuration.ServerUrl.IndexOf("remote.php", StringComparison.OrdinalIgnoreCase));
            foreach (var davItem in WebDavNavigationService.Items)
            {
                if (!davItem.IsCollection && davItem.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
                {
                    var itemPath = davItem.EntityId.Substring(davItem.EntityId.IndexOf("remote.php/webdav", StringComparison.OrdinalIgnoreCase) + 17);
                    var url = serverUrl + "index.php/apps/files/api/v1/thumbnail/" + 40 + "/" + 40 + itemPath;
                    davItem.ThumbnailUrl = url;
                }
            }
        }
    }
}
