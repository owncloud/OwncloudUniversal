using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using OwncloudUniversal.OwnCloud.Model;
using OwncloudUniversal.Services;
using OwncloudUniversal.Synchronization.Configuration;
using OwncloudUniversal.Synchronization.Model;
using OwncloudUniversal.Views;
using Template10.Mvvm;

namespace OwncloudUniversal.ViewModels
{
    class CameraUploadPageViewModel : ViewModelBase
    {
        private DavItem _targetFolder;
        private readonly InstantUploadRegistration _registration;
        public readonly ICommand SelectFolderCommand;
        public CameraUploadPageViewModel()
        {
            _targetFolder = new DavItem {EntityId = Configuration.CameraUploadTargetFolder};
            _registration = new InstantUploadRegistration();
            SelectFolderCommand = new DelegateCommand(() => NavigationService.NavigateAsync(typeof(SelectFolderPage), new DavItem {EntityId = Configuration.ServerUrl}, new SuppressNavigationTransitionInfo()));
        }

        public bool Enabled
        {
            get => _registration.Enabled;
            set
            {
                _registration.Enabled = value;
                if (value)
                {
                    DeleteCameraUploadAssociation();
                    CreateCameraUploadAssociation();
                    var backgroundSyncRegistration = new BackgroundTaskConfiguration();
                    backgroundSyncRegistration.Enabled = true;
                }
                else
                    DeleteCameraUploadAssociation();
            }
        }

        public DavItem TargetFolder
        {
            get { return _targetFolder; }
            private set
            {
                _targetFolder = value;
                Configuration.CameraUploadTargetFolder = value.EntityId;
                DeleteCameraUploadAssociation();
                CreateCameraUploadAssociation();
                RaisePropertyChanged();
            }
        }

        private async void CreateCameraUploadAssociation()
        {
            try
            {
                var library = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
                SyncedFoldersService service = new SyncedFoldersService();
                foreach (var folderAssociation in service.GetAllSyncedFolders())
                {
                    if (String.Equals(folderAssociation.LocalFolderPath, library.SaveFolder.Path, StringComparison.OrdinalIgnoreCase))
                    {
                        MessageDialog dialog = new MessageDialog(string.Format(App.ResourceLoader.GetString("SelectedFolderAlreadyInUseForSync"), library.SaveFolder.Path));
                        await dialog.ShowAsync();
                        _registration.Enabled = false;
                        RaisePropertyChanged("Enabled");
                        return;
                    }
                }
                await service.AddFolderToSyncAsync(library.SaveFolder, TargetFolder, true);
            }
            catch (Exception e)
            {
                Enabled = false;
                RaisePropertyChanged("Enabled");
                throw;
            }
        }

        public bool UploadViaWifiOnly
        {
            get => Configuration.UploadViaWifiOnly;
            set => Configuration.UploadViaWifiOnly = value;
        }

        private void DeleteCameraUploadAssociation()
        {
            new SyncedFoldersService().RemoveInstantUploadAssociations();
        }

        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (parameter is DavItem item && item.IsCollection)
            {
                TargetFolder = item;
            }
            return base.OnNavigatedToAsync(parameter, mode, state);
        }
    }
}
