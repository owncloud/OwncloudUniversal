using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Navigation;
using OwncloudUniversal.Shared.Model;
using Template10.Mvvm;
using Template10.Services.NavigationService;

namespace OwncloudUniversal.ViewModels
{
    public class SyncedFolderConfigurationPageViewModel : ViewModelBase
    {
        private FolderAssociation _association;

        public IEnumerable<SyncDirection> SyncDirections => Enum.GetValues(typeof(SyncDirection)).Cast<SyncDirection>();

        public FolderAssociation Association
        {
            get { return _association; }
            set
            {
                _association = value;
                RaisePropertyChanged();
            }
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (parameter is FolderAssociation)
                Association = (FolderAssociation) parameter;
            await base.OnNavigatedToAsync(parameter, mode, state);
        }

        public override Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            FolderAssociationTableModel.GetDefault().UpdateItem(Association, Association.Id);
            return base.OnNavigatingFromAsync(args);
        }
    }
}
