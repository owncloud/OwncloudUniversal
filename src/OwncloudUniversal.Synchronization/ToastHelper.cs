using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace OwncloudUniversal.Synchronization
{
    public static class ToastHelper
    {
        public static void SendToast(string message)
        {
#if DEBUG
            var xmlToastTemplate = "<toast launch=\"app-defined-string\">" +
                       "<visual>" +
                       "<binding template =\"ToastGeneric\">" +
                       "<text>" +
                       message +
                       "</text>" +
                       "</binding>" +
                       "</visual>" +
                       "</toast>";

            // load the template as XML document
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlToastTemplate);
            // create the toast notification and show to user
            var toastNotification = new ToastNotification(xmlDocument);
            var notification = ToastNotificationManager.CreateToastNotifier();
            notification.Show(toastNotification);
#endif
        }
    }
}
