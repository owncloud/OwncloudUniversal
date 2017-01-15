using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Common;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;
using OwncloudUniversal.Shared.Model;
using OwncloudUniversal.WebDav.Model;

namespace OwncloudUniversal.ViewModels
{
    public class DetailsPageViewModel : ViewModelBase
    {
        private DavItem _item;

        public DavItem Item
        {
            get { return _item; }
            private set
            {
                _item = value;
                RaisePropertyChanged();
            }
        }

        public DetailsPageViewModel()
        {
            
        }

        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            var item = parameter as AbstractItem;
            if (item != null)
                Item = (DavItem)item;
            return base.OnNavigatedToAsync(parameter, mode, state);
        }
    }
}

