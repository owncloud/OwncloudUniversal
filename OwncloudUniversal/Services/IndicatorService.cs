using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using OwncloudUniversal.Views;

namespace OwncloudUniversal.Services
{
    public class IndicatorService
    {
        private static IndicatorService _instance;

        private IndicatorService() { }

        public static IndicatorService GetDefault()
        {
            return _instance ?? (_instance = new IndicatorService());
        }

        public void ShowBar()
        {
            Shell.Ring.IsModal = true;
        }

        public void HideBar()
        {
            Shell.Ring.IsModal = false;
        }
    }
}
