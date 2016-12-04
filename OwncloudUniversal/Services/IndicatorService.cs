using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.UI.ViewManagement;

namespace OwncloudUniversal.Services
{
    public class InidcatorService
    {
        private static InidcatorService _instance;

        private InidcatorService() { }

        public static InidcatorService GetDefault()
        {
            return _instance ?? (_instance = new InidcatorService());
        }

        public async void ShowBar()
        {
            if (!ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar")) return;
            await StatusBar.GetForCurrentView().ProgressIndicator.ShowAsync();
        }

        public async void HideBar()
        {
            if (!ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar")) return;
            await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
        }
    }
}
