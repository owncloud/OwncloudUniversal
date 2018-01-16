using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using OwncloudUniversal.Services;
using OwncloudUniversal.Synchronization;
using OwncloudUniversal.Synchronization.Configuration;
using OwncloudUniversal.Synchronization.Model;
using OwncloudUniversal.Synchronization.Processing;
using Template10.Mvvm;

namespace OwncloudUniversal.ViewModels
{
    public class SynchronizationPageViewModel : ViewModelBase
    {
        private readonly SynchronizationService _syncService;
        BackgroundTaskConfiguration _taskConfig = new BackgroundTaskConfiguration();

        public ExecutionContext ExecutionContext => ExecutionContext.Instance;
        public ICommand StartSyncCommand { get; private set; }
        public ICommand ResumeSyncCommand { get; private set; }

        public SynchronizationPageViewModel()
        {
            _syncService = SynchronizationService.GetInstance();
            StartSyncCommand = new DelegateCommand(async () =>
            {
                await _syncService.StartSyncProcess();
            });
            ResumeSyncCommand = new DelegateCommand(() =>
            {
                
                _syncService.ResumeSyncProcess();
            });
        }

        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            LoadThumbnails();
            return base.OnNavigatedToAsync(parameter, mode, state);
        }

        public bool BackgroundTaskEnabled
        {
            get { return _taskConfig.Enabled; }
            set
            { 
                Configuration.IsBackgroundTaskEnabled = value;
                _taskConfig.Enabled = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<SyncHistoryEntry> HistoryEntries { get; } = SyncHistoryTableModel.GetDefault().GetAllItems();

        private async void LoadThumbnails()
        {

            var serverUrl = Configuration.ServerUrl.Substring(0, Configuration.ServerUrl.IndexOf("remote.php", StringComparison.OrdinalIgnoreCase));
            foreach (var syncHistoryEntry in HistoryEntries)
            {
                try
                {
                    if(syncHistoryEntry.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
                    {
                        if (syncHistoryEntry.EntityId.Contains("/"))
                        {

                            var itemPath = syncHistoryEntry.EntityId.Substring(syncHistoryEntry.EntityId.IndexOf("remote.php/webdav", StringComparison.OrdinalIgnoreCase) + 17);
                            var url = serverUrl + "index.php/apps/files/api/v1/thumbnail/" + 40 + "/" + 40 + itemPath;
                            syncHistoryEntry.Image = new BitmapImage(new Uri(url));
                        }
                        else
                        {
                            var file = await StorageFile.GetFileFromPathAsync(syncHistoryEntry.EntityId);
                            var thumb = await file.GetThumbnailAsync(ThumbnailMode.PicturesView, 40, ThumbnailOptions.None);
                            syncHistoryEntry.Image = new BitmapImage();
                            syncHistoryEntry.Image.SetSource(thumb);
                        }

                        RaisePropertyChanged("Image");
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }
    }
}
