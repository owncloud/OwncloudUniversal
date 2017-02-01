using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OwncloudUniversal.Shared.Model;
using OwncloudUniversal.WebDav.Model;

namespace OwncloudUniversal.Services
{
    interface IWebDavNavigationService
    {
        ObservableCollection<DavItem> FolderStack { get; } 
        ObservableCollection<DavItem> Items { get; }
        DavItem CurrentItem { get; }
        Task NavigateAsync(DavItem item);
        Task GoForwardAsync();
        Task GoBackAsync();
        Task ReloadAsync();
    }
}
