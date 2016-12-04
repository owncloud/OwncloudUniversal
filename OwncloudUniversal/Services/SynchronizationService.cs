using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using OwncloudUniversal.Shared;
using OwncloudUniversal.Shared.LocalFileSystem;
using OwncloudUniversal.Shared.Synchronisation;
using OwncloudUniversal.WebDav;
using Template10.Mvvm;

namespace OwncloudUniversal.Services
{
    class SynchronizationService
    {
        public readonly BackgroundSyncProcess Worker;

        public SynchronizationService()
        {
            var fileSystem = new FileSystemAdapter(false, null);
            var webDav = new WebDavAdapter(false, Configuration.ServerUrl, Configuration.Credential, fileSystem);
            fileSystem.LinkedAdapter = webDav;
            Worker = new BackgroundSyncProcess(fileSystem, webDav, false);
            
        }

        public async Task StartSyncProcess()
        {
            InidcatorService.GetDefault().ShowBar();
            await Worker.Run();
            InidcatorService.GetDefault().HideBar();
        }
    }
}
