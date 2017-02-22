using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using OwncloudUniversal.Services;
using OwncloudUniversal.Shared;
using OwncloudUniversal.Shared.Synchronisation;
using Template10.Mvvm;

namespace OwncloudUniversal.ViewModels
{
    public class SynchronizationPageViewModel : ViewModelBase
    {
        private readonly SynchronizationService _syncService;
        private bool _isActive;

        public ExecutionContext ExecutionContext => _syncService.Worker.ExecutionContext;
        public ICommand StartSyncCommand { get; private set; }

        public bool IsActive
        {
            get { return _isActive; }
            private set
            {
                _isActive = value;
                RaisePropertyChanged();
            }
        }

        public SynchronizationPageViewModel()
        {
            _syncService = SynchronizationService.GetInstance();
            StartSyncCommand = new DelegateCommand(async () =>
            {
                try
                {
                    IsActive = true;
                    await _syncService.StartSyncProcess();
                }
                finally
                {
                    IsActive = false;
                }
            });
        }

        public bool BackgroundTaskEnabled
        {
            get { return Configuration.IsBackgroundTaskEnabled; }
            set
            { 
                Configuration.IsBackgroundTaskEnabled = value;
            }
        }
    }
}
