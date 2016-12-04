using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using OwncloudUniversal.Services;
using OwncloudUniversal.Shared.Synchronisation;
using Template10.Mvvm;

namespace OwncloudUniversal.ViewModels
{
    public class SynchronizationPageViewModel : ViewModelBase
    {
        private readonly SynchronizationService _syncService;

        public ExecutionContext ExecutionContext => _syncService.Worker.ExecutionContext;
        public ICommand StartSyncCommand { get; private set; }


        public SynchronizationPageViewModel()
        {
            _syncService = new SynchronizationService();
            StartSyncCommand = new DelegateCommand(async () => await _syncService.StartSyncProcess());
        }
    }
}
