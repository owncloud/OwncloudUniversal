using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.AppService;
using Windows.Media.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using OwncloudUniversal.Services;
using OwncloudUniversal.Shared;
using OwncloudUniversal.Shared.Model;
using OwncloudUniversal.Views;
using OwncloudUniversal.WebDav;
using OwncloudUniversal.WebDav.Model;
using Template10.Mvvm;
using Template10.Services.NavigationService;

namespace OwncloudUniversal.ViewModels
{
    public class PhotoPageViewModel : ViewModelBase
    {
        private List<DavItem> _items;
        private int _selectedIndex;
        private bool _showControls;
        private WebDavClient _client;
        private MediaSource _mediaSource;

        public PhotoPageViewModel()
        {
            EnterFullScreenCommand = new DelegateCommand(EnterFullScreen);
            LeaveFullScreenCommand = new DelegateCommand(LeaveFullscreen);
            ShowControls = true;
            _client = new WebDavClient(new Uri(Configuration.ServerUrl), Configuration.Credential);
        }

        public ICommand EnterFullScreenCommand { get; private set; }
        public ICommand LeaveFullScreenCommand { get; private set; }
        public List<DavItem> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                RaisePropertyChanged();
            }
        }
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
                RaisePropertyChanged();
                RaisePropertyChanged("SelectedItem");
            }
        }
        public bool IsInFullScreen => ApplicationView.GetForCurrentView().IsFullScreenMode;
        private void EnterFullScreen()
        {
            ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
            RaisePropertyChanged("IsInFullScreen");
        }

        private void LeaveFullscreen()
        {
            ApplicationView.GetForCurrentView().ExitFullScreenMode();
            RaisePropertyChanged("IsInFullScreen");
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            Items = await LoadItems();
            PropertyChanged += async (sender, args) =>
            {
                if (args.PropertyName == "SelectedItem" && (SelectedItem?.ContentType.StartsWith("video") ?? false))
                    await CreateMediaStream(SelectedItem);
                else
                {
                    MediaSource?.Reset();
                }
            };
            SelectedIndex = Items.FindIndex(i => i.EntityId == ((DavItem)parameter).EntityId);
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                EnterFullScreen();
            Shell.HamburgerMenu.IsFullScreen = true;
            await base.OnNavigatedToAsync(parameter, mode, state);
        }

        public DavItem SelectedItem
        {
            get
            {
                if (SelectedIndex != -1 && Items != null)
                {
                    return Items[SelectedIndex];
                }
                return null;
            }
        }

        public MediaSource MediaSource
        {
            get { return _mediaSource; }
            set
            {
                _mediaSource = value;
                RaisePropertyChanged();
            }
        }

        private async Task CreateMediaStream(DavItem item)
        {
            var stream = await _client.GetAsHttpRandomAccessStream(item);
            MediaSource = MediaSource.CreateFromStream(stream, stream.ContentType);
        }

        private async Task<List<DavItem>>  LoadItems()
        {
            var service = await WebDavNavigationService.InintializeAsync();
            var serverUrl = Configuration.ServerUrl.Substring(0, Configuration.ServerUrl.IndexOf("remote.php", StringComparison.OrdinalIgnoreCase));
            foreach (var davItem in service.Items)
            {
                if (!davItem.IsCollection && davItem.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
                {
                    var itemPath = davItem.EntityId.Substring(davItem.EntityId.IndexOf("remote.php/webdav", StringComparison.OrdinalIgnoreCase) + 17);
                    var url = serverUrl + "index.php/apps/files/api/v1/thumbnail/" + 2000 + "/" + 2000 + itemPath;
                    davItem.ThumbnailUrl = url;
                }
            }
            return service.Items.Where(i => i.ContentType.StartsWith("image") || i.ContentType.StartsWith("video")).ToList();
        }

        public override Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            Shell.HamburgerMenu.IsFullScreen = false;
            LeaveFullscreen();
            return base.OnNavigatingFromAsync(args);
        }

        public bool ShowControls
        {
            get { return _showControls; }
            set
            {
                _showControls = value;
                RaisePropertyChanged();
            }
        }
    }
}
