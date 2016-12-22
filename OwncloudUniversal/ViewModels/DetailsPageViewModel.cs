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

namespace OwncloudUniversal.ViewModels
{
    public class DetailsPageViewModel : ViewModelBase
    {
        public AbstractItem Item { get; private set; }
        public DetailsPageViewModel()
        {
            
        }

        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            var item = parameter as AbstractItem;
            if (item != null)
                Item = item;
            return base.OnNavigatedToAsync(parameter, mode, state);
        }
    }
}

