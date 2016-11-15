using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using OwncloudUniversal.Shared;
using OwncloudUniversal.Shared.LocalFileSystem;
using OwncloudUniversal.Shared.Synchronisation;
using OwncloudUniversal.WebDav;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace OwncloudUniversal.UI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SyncMonitor : Page
    {
        public SyncWorker Worker { get; set; }
        public SyncMonitor()
        {
            this.InitializeComponent();
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += BackRequestet;

            var fileSystem = new FileSystemAdapter(false, null);
            var webDav = new WebDavAdapter(false, Configuration.ServerUrl, Configuration.Credential, fileSystem);
            fileSystem.LinkedAdapter = webDav;
            Worker = new SyncWorker(fileSystem,webDav, false);
        }

        private void BackRequestet(object sender, BackRequestedEventArgs args)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested -= BackRequestet;
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
                return;
            if (rootFrame.CanGoBack && args.Handled == false)
            {
                args.Handled = true;
                rootFrame.GoBack();
            }
        }

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!Configuration.CurrentlyActive)
            {
                DisplayRequest request = new DisplayRequest();
                request.RequestActive();
                try
                {
                    progressBar.IsIndeterminate = true;
                    progressBar.Visibility = Visibility.Visible;
                    Configuration.CurrentlyActive = true;
                    await Worker.Run();
                    Configuration.CurrentlyActive = false;
                }
                finally
                {
                    progressBar.IsIndeterminate = false;
                    progressBar.Visibility = Visibility.Collapsed;
                    Configuration.CurrentlyActive = false;
                    request.RequestRelease();
                }
            }
            else
            {
                Worker.ExecutionContext.Status = ExecutionStatus.Stopped;
            }
            
        }
    }
}
