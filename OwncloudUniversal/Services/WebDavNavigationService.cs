using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using OwncloudUniversal.WebDav.Model;
using Template10.Utils;

namespace OwncloudUniversal.Services
{
    public class WebDavNavigationService : IWebDavNavigationService, INotifyPropertyChanged
    {
        private static WebDavNavigationService _instance;
        private readonly WebDavItemService _itemService;
        private DavItem _currentItem;
        private ObservableCollection<DavItem> _items;

        private WebDavNavigationService()
        {
            _itemService = WebDavItemService.GetDefault();
            FolderStack = new ObservableCollection<DavItem>();
            Items = new ObservableCollection<DavItem>();
        }
        public static WebDavNavigationService GetDefault() => _instance ?? (_instance = new WebDavNavigationService());

        public ObservableCollection<DavItem> FolderStack { get; private set; }

        public ObservableCollection<DavItem> Items
        {
            get { return _items; }
            private set
            {
                _items = value;
                OnPropertyChanged();
            }
        }

        public DavItem CurrentItem
        {
            get { return _currentItem; }
            private set
            {
                _currentItem = value;
                OnPropertyChanged();
            }
        }

        public async Task NavigateAsync(DavItem item)
        {
            FolderStack.Add(item);
            CurrentItem = item;
            await ReloadAsync();
        }

        public Task GoForwardAsync()
        {
            throw new NotImplementedException();
        }

        public async Task GoBackAsync()
        {
            FolderStack.RemoveAt(FolderStack.Count-1);
            CurrentItem = FolderStack.Last();
            await ReloadAsync();
        }

        public async Task ReloadAsync()
        {
            IndicatorService.GetDefault().ShowBar();
            Items = null;
            Items = (await _itemService.GetItemsAsync(new Uri(CurrentItem.EntityId, UriKind.RelativeOrAbsolute))).ToObservableCollection();
            IndicatorService.GetDefault().HideBar();
        }

        public Task ClearHistory()
        {
            CurrentItem = null;
            Items = new ObservableCollection<DavItem>();
            FolderStack = new ObservableCollection<DavItem>();
            return Task.CompletedTask;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
