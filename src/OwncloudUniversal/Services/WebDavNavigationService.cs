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
using OwncloudUniversal.Synchronization;
using OwncloudUniversal.Synchronization.Model;
using OwncloudUniversal.Views;
using OwncloudUniversal.OwnCloud.Model;
using OwncloudUniversal.Synchronization.Configuration;
using Template10.Common;
using Template10.Services.NavigationService;
using Template10.Utils;

namespace OwncloudUniversal.Services
{
    public class WebDavNavigationService : INotifyPropertyChanged
    {
        private static WebDavNavigationService _instance;
        private static INavigationService _navigationService;
        private readonly WebDavItemService _itemService;
        private DavItem _currentItem;
        private ObservableCollection<DavItem> _items;
        private ObservableCollection<DavItem> _backStack;
        private ObservableCollection<DavItem> _forwardStack;

        private WebDavNavigationService()
        {
            _navigationService = BootStrapper.Current.NavigationService;
            _navigationService.FrameFacade.BackRequested += OnBackRequested;
            _navigationService.FrameFacade.ForwardRequested += OnForwardRequested;
            _itemService = WebDavItemService.GetDefault();
            BackStack = new ObservableCollection<DavItem>();
            ForwardStack = new ObservableCollection<DavItem>();
            Items = new ObservableCollection<DavItem>();
        }

        private async void OnForwardRequested(object sender, HandledEventArgs handledEventArgs)
        {
            var source = _navigationService.FrameFacade.Frame.ForwardStack.LastOrDefault();
            if(source != null)
                if (!(source.SourcePageType == typeof(FilesPage) || source.SourcePageType == typeof(SelectFolderPage)))
                    return;
            if (ForwardStack.Count > 0)
            {
                handledEventArgs.Handled = true;
                await GoForwardAsync();
            }
        }

        private async void OnBackRequested(object sender, HandledEventArgs handledEventArgs)
        {
            var source = _navigationService.FrameFacade.Frame;
            if(source != null)
                if (!(source.SourcePageType == typeof(FilesPage) || source.SourcePageType == typeof(SelectFolderPage)))
                    return;

            if (BackStack.Count > 1)
            {
                handledEventArgs.Handled = true;
                await GoBackAsync();
            }
        }

        public static async Task<WebDavNavigationService> InintializeAsync()
        {
            if (_instance != null) return _instance;
            _instance = new WebDavNavigationService();
            var item = new DavItem { Href = Configuration.ServerUrl, IsCollection = true };
            try
            {
                await _instance.NavigateAsync(item);
            }
            catch (Exception e)
            {
                await ExceptionHandlerService.HandleException(e, e.Message);
            }
            return _instance;
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
            get { return _backStack; }
            private set
            {
                _backStack = value;
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
            if(item == null)
                return;
            if (BackStack.FirstOrDefault(x=>x.EntityId == item.EntityId) != null)
            {
                var backStackItem = BackStack.FirstOrDefault(x => x.EntityId == item.EntityId);
                int index = BackStack.IndexOf(backStackItem);
                var list = BackStack.ToList();
                list.RemoveRange(index+1, BackStack.Count - index-1);
                BackStack = list.ToObservableCollection();
                ForwardStack.Clear();
                CurrentItem = backStackItem;
            }
            else
            {
                BackStack.Add(item);
                ForwardStack.Remove(item);
                CurrentItem = item;
            }
            await ReloadAsync();
        }

        private async Task GoForwardAsync()
        {
           if(ForwardStack.Count > 0)
                CurrentItem = ForwardStack.Last();
            ForwardStack.Remove(CurrentItem);
            BackStack.Add(CurrentItem);
            await ReloadAsync();
        }

        private async Task GoBackAsync()
        {
            BackStack.Remove(CurrentItem);
            ForwardStack.Add(CurrentItem);
            if (BackStack.Count > 0)
                CurrentItem = BackStack.Last();
            await ReloadAsync();
        }

        public async Task ReloadAsync()
        {
            IndicatorService.GetDefault().ShowBar();
            Items.Clear();
            Items.AddRange(await _itemService.GetItemsAsync(new Uri(CurrentItem.EntityId, UriKind.RelativeOrAbsolute)));
            OnPropertyChanged("Items");
            IndicatorService.GetDefault().HideBar();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
