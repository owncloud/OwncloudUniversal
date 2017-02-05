using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using OwncloudUniversal.Services;
using OwncloudUniversal.Shared;
using OwncloudUniversal.Views;
using OwncloudUniversal.WebDav;
using OwncloudUniversal.WebDav.Model;
using Template10.Mvvm;
using Template10.Services.NavigationService;

namespace OwncloudUniversal.ViewModels
{
    public class SelectFolderPageViewModel : ViewModelBase
    {
        private DavItem _selectedItem;
        private static List<DavItem> _itemsToMove;
        private WebDavNavigationService _webDavNavigationService;

        public WebDavNavigationService WebDavNavigationService
        {
            get { return _webDavNavigationService; }
            private set
            {
                _webDavNavigationService = value;
                RaisePropertyChanged();
            }
        }

        public ICommand HomeCommand { get; private set; }
        public ICommand AcceptCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public SelectFolderPageViewModel()
        {
            HomeCommand = new DelegateCommand(async () => await NavigationService.NavigateAsync(typeof(SelectFolderPage), new DavItem{EntityId = Configuration.ServerUrl}, new SuppressNavigationTransitionInfo()));
            AcceptCommand = new DelegateCommand(async () => await Move());
            CancelCommand = new DelegateCommand(async () => await NavigationService.NavigateAsync(typeof(FilesPage)));
        }
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            WebDavNavigationService = await WebDavNavigationService.InintializeAsync();
            WebDavNavigationService.SetNavigationService(NavigationService);
            if (parameter is List<DavItem>)
            {
                _itemsToMove = parameter as List<DavItem>;
            }
            await base.OnNavigatedToAsync(parameter, mode, state);
        }

        private async Task Move()
        {
            try
            {
                foreach (var davItem in _itemsToMove)
                {
                    await WebDavItemService.GetDefault().MoveToFolder(davItem, WebDavNavigationService.CurrentItem);
                }
            }
            finally
            {
                await NavigationService.NavigateAsync(typeof(FilesPage), WebDavNavigationService.CurrentItem, new SuppressNavigationTransitionInfo());
            }
        }

        public DavItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (_selectedItem == value)
                    return;
                _selectedItem = value;
                RaisePropertyChanged();
                if (value.IsCollection)
                {
                    NavigationService.Navigate(typeof(SelectFolderPage), value, new SuppressNavigationTransitionInfo());
                }
            }
            
        }
    }
}
