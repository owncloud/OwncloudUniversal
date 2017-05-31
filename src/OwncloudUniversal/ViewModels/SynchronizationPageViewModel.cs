using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using OwncloudUniversal.Services;
using OwncloudUniversal.Synchronization;
using OwncloudUniversal.Synchronization.Configuration;
using OwncloudUniversal.Synchronization.Synchronisation;
using Template10.Mvvm;

namespace OwncloudUniversal.ViewModels
{
    public class SynchronizationPageViewModel : ViewModelBase
    {
        private readonly SynchronizationService _syncService;

        public ExecutionContext ExecutionContext => ExecutionContext.Instance;
        public ICommand StartSyncCommand { get; private set; }

        public SynchronizationPageViewModel()
        {
            _syncService = SynchronizationService.GetInstance();
            StartSyncCommand = new DelegateCommand(async () =>
            {

                await _syncService.StartSyncProcess();
            });
        }

        public bool BackgroundTaskEnabled
        {
            get { return Configuration.IsBackgroundTaskEnabled; }
            set
            { 
                Configuration.IsBackgroundTaskEnabled = value;
                RaisePropertyChanged();
            }
        }
    }
}
