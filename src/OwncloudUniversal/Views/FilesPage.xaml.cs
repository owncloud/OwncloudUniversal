using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Input;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using OwncloudUniversal.Synchronization.Model;
using OwncloudUniversal.ViewModels;
using OwncloudUniversal.OwnCloud.Model;
using Template10.Services.PopupService;
using Template10.Utils;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace OwncloudUniversal.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FilesPage : Page
    {
        public FilesPageViewModel FilesPageViewModel { get; } = new FilesPageViewModel();
        private static List<object> _selectedItems { get; set; }


        public static List<DavItem> GetSelectedItems(DavItem item)
        {
            //workaround because if multiple selected Items are passed 
            //directly as commandparameter they are always null
            if (_selectedItems == null || _selectedItems.Count <= 1)
                return new List<DavItem>() {item};
            return _selectedItems?.Cast<DavItem>().ToList();
        }

        public FilesPage()
        {
            this.InitializeComponent();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(sender is ListView)
                _selectedItems = ListView.SelectedItems?.ToList();
            if (sender is GridView)
                _selectedItems = GridView.SelectedItems?.ToList();
        }

        private void ItemGrid_OnHolding(object sender, HoldingRoutedEventArgs e)
        {
            if(e.HoldingState == HoldingState.Started)
            {
                var senderElement = sender as UIElement;
                MenuFlyout flyoutBase = (MenuFlyout)FlyoutBase.GetAttachedFlyout((FrameworkElement)senderElement);
                flyoutBase.ShowAt(senderElement, e.GetPosition(senderElement));
                Debug.WriteLine("Holding " + e.GetPosition(senderElement));
            }
        }

        private void ItemGrid_OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (UIViewSettings.GetForCurrentView().UserInteractionMode == UserInteractionMode.Mouse)
            {
                var senderElement = sender as UIElement;
                MenuFlyout flyoutBase = (MenuFlyout)FlyoutBase.GetAttachedFlyout((FrameworkElement)senderElement);
                flyoutBase.ShowAt(senderElement, e.GetPosition(senderElement));
                Debug.WriteLine("Tapped " + e.GetPosition(senderElement));
            }
        }
    }
}
