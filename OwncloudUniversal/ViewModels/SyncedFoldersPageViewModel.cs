using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Navigation;
using OwncloudUniversal.Services;
using OwncloudUniversal.Shared.Model;
using Template10.Mvvm;

namespace OwncloudUniversal.ViewModels
{
    public class SyncedFoldersPageViewModel : ViewModelBase
    {
        private readonly SyncedFoldersService _syncedFoldersService;
        private List<FolderAssociation> _syncedFolders;
        public FolderAssociation SelectedItem { get; set; }
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
            SyncedFolders = _syncedFoldersService.GetAllSyncedFolders();
        }

        private void RemoveFromSync(object parameter)
        {
            if (parameter is FolderAssociation)
            {
                AbstractItemTableModel.GetDefault().DeleteItemsFromAssociation((FolderAssociation)parameter);
                FolderAssociationTableModel.GetDefault().DeleteItem(((FolderAssociation)parameter).Id);
                LoadFolders();
            }
        }
    }
}
