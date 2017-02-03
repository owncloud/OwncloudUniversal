using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OwncloudUniversal.Shared.Model;
using OwncloudUniversal.WebDav.Model;
using Template10.Services.NavigationService;

namespace OwncloudUniversal.Services
{
    interface IWebDavNavigationService
    {
        ObservableCollection<DavItem> BackStack { get; } 
        ObservableCollection<DavItem> ForwardStack { get; }
        DavItem CurrentItem { get; }
        Task NavigateAsync(DavItem item);
        Task GoForwardAsync();
        Task GoBackAsync();
        Task ReloadAsync();
        Task Reset();
        void SetNavigationService(INavigationService service);
    }
}
