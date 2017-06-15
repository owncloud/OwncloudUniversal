using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using OwncloudUniversal.Synchronization;
using OwncloudUniversal.Synchronization.Configuration;

namespace OwncloudUniversal.Utils
{
    class DesktopClientHelper
    {
        public static async Task ShowDekstopClientInfo()
        {
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop")
            {
                bool installed = false;
                try
                {
                    installed = Directory.Exists("C:\\Program Files (x86)\\ownCloud") ||
                                Directory.Exists("C:\\Program Files\\ownCloud");
                }
                catch (Exception e)
                {
                    await LogHelper.Write(e.Message);
                }
                if (!installed && !Configuration.HideDesktopClientInfo)
                {
                    var dialog = new MessageDialog(App.ResourceLoader.GetString("DesktopClientInfoText"));
                    dialog.Commands.Add(new UICommand(App.ResourceLoader.GetString("Yes")) { Id = 0 });
                    dialog.Commands.Add(new UICommand(App.ResourceLoader.GetString("No")) { Id = 1 });
                    dialog.Commands.Add(new UICommand(App.ResourceLoader.GetString("DontShowAgain")) { Id = 2 });
                    var result = await dialog.ShowAsync();
                    if ((int)result.Id== 0)
                    {
                        await Launcher.LaunchUriAsync(new Uri("https://owncloud.org/install/#install-clients"));
                        App.Current.Exit();
                    }
                    if ((int)result.Id == 1)
                    {
                        Configuration.HideDesktopClientInfo = false;
                    }
                    if ((int)result.Id == 2)
                    {
                        Configuration.HideDesktopClientInfo = true;
                    }
                }
            }
        }
    }
}
