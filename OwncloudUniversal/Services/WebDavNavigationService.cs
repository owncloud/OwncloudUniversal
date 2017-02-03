using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using OwncloudUniversal.Shared;
using OwncloudUniversal.Shared.Model;
using OwncloudUniversal.Views;
using OwncloudUniversal.WebDav.Model;
using Template10.Services.NavigationService;
using Template10.Utils;

namespace OwncloudUniversal.Services
{
    public class WebDavNavigationService : IWebDavNavigationService, INotifyPropertyChanged
    {
        private static WebDavNavigationService _instance;
        private readonly WebDavItemService _itemService;
        private DavItem _currentItem;
        private ObservableCollection<DavItem> _items;
        private ObservableCollection<DavItem> _folderStack;
        private ObservableCollection<DavItem> _forwardStack;
        private INavigationService _navigationService;

        private WebDavNavigationService()
        {
            _itemService = WebDavItemService.GetDefault();
            BackStack = new ObservableCollection<DavItem>();
            ForwardStack = new ObservableCollection<DavItem>();
            Items = new ObservableCollection<DavItem>();
            var item = new DavItem{ Href = Configuration.ServerUrl, IsCollection = true };
#pragma warning disable 4014
            NavigateAsync(item);
#pragma warning restore 4014
        }
        public static WebDavNavigationService GetDefault() => _instance ?? (_instance = new WebDavNavigationService());

        public void SetNavigationService(INavigationService service)
        {
            _navigationService = service;
        }

        public ObservableCollection<DavItem> Items
        {
            get { return _items; }
            private set
            {
                _items = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<DavItem> BackStack
        {
            get { return _folderStack; }
            private set
            {
                _folderStack = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<DavItem> ForwardStack
        {
            get { return _forwardStack; }
            private set
            {
                _forwardStack = value;
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
            if (BackStack.Contains(item))
            {
                var entry = _navigationService?.Frame.BackStack.FirstOrDefault(x =>
                {
                    var s = x.Parameter as string;
                    if (s == null) return false;
                    if (!s.Contains(item.GetType().FullName)) return false;
                    var parameterItem = Template10.Services.SerializationService.SerializationService.Json.Deserialize(s);
                    return ((DavItem)parameterItem).EntityId == item.EntityId;
                });
                if (entry != default(PageStackEntry))
                {
                    int i = _navigationService.Frame.BackStack.IndexOf(entry);
                    while (_navigationService.Frame.BackStackDepth > i)
                    {
                        _navigationService.Frame.BackStack.RemoveAt(_navigationService.Frame.BackStackDepth-1);
                    }
                }
                else _navigationService?.Frame.BackStack.Clear();
                int index = BackStack.IndexOf(item);
                var list = BackStack.ToList();
                list.RemoveRange(index+1, BackStack.Count - index-1);
                BackStack = list.ToObservableCollection();
                ForwardStack.Clear();
                CurrentItem = item;
            }
            else
            {
                BackStack.Add(item);
                ForwardStack.Remove(item);
                CurrentItem = item;
            }
            await ReloadAsync();
        }

        public async Task GoForwardAsync()
        {
            CurrentItem = ForwardStack.Last();
            ForwardStack.Remove(CurrentItem);
            BackStack.Add(CurrentItem);
            await ReloadAsync();
        }

        public async Task GoBackAsync()
        {
            BackStack.Remove(CurrentItem);
            ForwardStack.Add(CurrentItem);
            CurrentItem = BackStack.Last();
            await ReloadAsync();
        }

        public async Task ReloadAsync()
        {
            IndicatorService.GetDefault().ShowBar();
            Items = null;
            Items = (await _itemService.GetItemsAsync(new Uri(CurrentItem.EntityId, UriKind.RelativeOrAbsolute))).ToObservableCollection();
            IndicatorService.GetDefault().HideBar();
        }

        public Task Reset()
        {
            _instance = new WebDavNavigationService();
            return Task.CompletedTask;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
