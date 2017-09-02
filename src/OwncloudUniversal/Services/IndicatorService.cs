using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using OwncloudUniversal.Views;
using Template10.Common;

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
            Shell.ModalDialog.IsModal = true;
            Shell.ModalDialog.ModalBackground.Opacity = 0;
        }

        public void ShowBar(string text, UIElement element = null)
        {
            Shell.ModalDialog.ModalBackground = (SolidColorBrush) BootStrapper.Current.Resources["OverlayColor"];
            Shell.ModalDialog.ModalBackground.Opacity = 0.75;
            Shell.ModalDialog.IsModal = true;
            Shell.Text.Text = text;
            var panel = Shell.ModalDialog.ModalContent as StackPanel;
            if(panel?.Children.Contains(element) == false && element != null)
                panel.Children.Add(element);
        }

        public void HideBar()
        {
            Shell.ModalDialog.IsModal = false;
            Shell.ModalDialog.ModalBackground.Opacity = 0;
            Shell.Text.Text = string.Empty;
            var panel = Shell.ModalDialog.ModalContent as StackPanel;
            if(panel != null && panel.Children.Count > 2)
                panel.Children.Remove(panel.Children.Last());
        }
    }
}
