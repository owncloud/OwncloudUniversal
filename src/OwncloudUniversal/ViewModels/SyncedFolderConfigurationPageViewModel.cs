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
using OwncloudUniversal.Synchronization.Synchronisation;
using OwncloudUniversal.Views;
using Template10.Mvvm;
using Template10.Services.NavigationService;

namespace OwncloudUniversal.ViewModels
{
    public class SyncedFolderConfigurationPageViewModel : ViewModelBase
    {
        private FolderAssociation _association;
        private readonly SynchronizationService _syncService;

        public SyncedFolderConfigurationPageViewModel()
        {
            DeleteCommand = new DelegateCommand(Delete);
            SaveCommand = new DelegateCommand(Save);
            _syncService = SynchronizationService.GetInstance();
            StartSyncCommand = new DelegateCommand(async () =>
            {
                Save();
                await NavigationService.NavigateAsync(typeof(SynchronizationPage));
                await _syncService.StartSyncProcess();
            });
        }

        public IEnumerable<SyncDirection> SyncDirections => Enum.GetValues(typeof(SyncDirection)).Cast<SyncDirection>();

        public FolderAssociation Association
        {
            get { return _association; }
            set
            {
                _association = value;
                RaisePropertyChanged();
            }
        }
        public ICommand DeleteCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }

        public ExecutionContext ExecutionContext => ExecutionContext.Instance;
        public ICommand StartSyncCommand { get; private set; }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (parameter is FolderAssociation)
                Association = (FolderAssociation) parameter;
            await base.OnNavigatedToAsync(parameter, mode, state);
        }

        private void Delete()
        {
            var service = new SyncedFoldersService();
            service.RemoveFromSyncedFolders(Association);
            NavigationService.Navigate(typeof(SyncedFoldersPageView), null, new SuppressNavigationTransitionInfo());
        }

        private void Save()
        {
            FolderAssociationTableModel.GetDefault().UpdateItem(_association, _association.Id);
        }
    }
}
