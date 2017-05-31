using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace OwncloudUniversal.Utils
{
    public static class ThemeHelper
    {
        public static void UpdateTitleBar()
        {
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
            {
                var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                if (titleBar != null)
                {
                    titleBar.BackgroundColor = (Color)Application.Current.Resources["CustomColor"];
                    titleBar.ForegroundColor = Colors.White;
                    titleBar.ButtonBackgroundColor = (Color)Application.Current.Resources["CustomColor"];
                    titleBar.ButtonForegroundColor = Colors.White;
                }
            }

            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {

                var statusBar = StatusBar.GetForCurrentView();
                if (statusBar != null)
                {
                    statusBar.BackgroundOpacity = 1;
                    statusBar.BackgroundColor = (Color)Application.Current.Resources["CustomColor"];
                    statusBar.ForegroundColor = Colors.White;
                }
            }
        }
    }
}
