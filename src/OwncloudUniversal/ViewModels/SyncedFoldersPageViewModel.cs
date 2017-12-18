using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using OwncloudUniversal.Services;
using OwncloudUniversal.Synchronization.Model;
using OwncloudUniversal.Views;
using Template10.Mvvm;

namespace OwncloudUniversal.ViewModels
{
    public class SyncedFoldersPageViewModel : ViewModelBase
    {
        private readonly SyncedFoldersService _syncedFoldersService;
        private List<FolderAssociation> _syncedFolders;
        private FolderAssociation _selectedItem;

        public FolderAssociation SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                if(!value.SupportsInstantUpload)
                    NavigationService.Navigate(typeof(SyncedFolderConfigurationPage), _selectedItem, new SuppressNavigationTransitionInfo());
                else
                {
                    NavigationService.Navigate(typeof(CameraUploadPage), null, new SuppressNavigationTransitionInfo());
                }
            }
        }

        public ICommand RemoveFromSyncCommand { get; private set; }

        public SyncedFoldersPageViewModel()
        {
            _syncedFoldersService = new SyncedFoldersService();
            RemoveFromSyncCommand = new DelegateCommand<object>(RemoveFromSync);
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            await base.OnNavigatedToAsync(parameter, mode, state);
            LoadFolders();
        }

        public List<FolderAssociation> SyncedFolders
        {
            get { return _syncedFolders; }
            private set { _syncedFolders = value; RaisePropertyChanged(); }
        }

        private void LoadFolders()
        {
            SyncedFolders = _syncedFoldersService.GetConfiguredFolders();
        }

        private void RemoveFromSync(object parameter)
        {
            var association = parameter as FolderAssociation;
            if (association != null)
            {
                _syncedFoldersService.RemoveFromSyncedFolders(association);
                LoadFolders();
            }
        }
    }
}
