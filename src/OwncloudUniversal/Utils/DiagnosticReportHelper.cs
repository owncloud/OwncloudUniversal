using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Email;
using Windows.Networking.Connectivity;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage;
using Windows.System.Profile;
using OwncloudUniversal.Shared;

namespace OwncloudUniversal.Utils
{
    static class DiagnosticReportHelper
    {
        public static async Task GenerateReport()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("**************************************************");
            builder.AppendLine("ownCloud Universal Windows App - Diagnostic Report");
            builder.AppendLine("**************************************************");
            builder.AppendLine($"Name: {Package.Current.DisplayName}");
            var v = Windows.ApplicationModel.Package.Current.Id.Version;
            builder.AppendLine($"Version: { v.Major}.{ v.Minor}.{ v.Build}.{ v.Revision}");
            builder.AppendLine($"Architecture: {Package.Current.Id.Architecture}");
            builder.AppendLine($"DevelopmentMode: {Package.Current.IsDevelopmentMode}");
            builder.AppendLine($"InstallDate: {Package.Current.InstalledDate.ToUniversalTime()}");
            builder.AppendLine($"Culture: {CultureInfo.CurrentCulture.Name}");
            builder.AppendLine($"DeviceFamily: {Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily}");

            string sv = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
            ulong version = ulong.Parse(sv);
            ulong v1 = (version & 0xFFFF000000000000L) >> 48;
            ulong v2 = (version & 0x0000FFFF00000000L) >> 32;
            ulong v3 = (version & 0x00000000FFFF0000L) >> 16;
            ulong v4 = (version & 0x000000000000FFFFL);
            builder.AppendLine($"OS Version: {v1}.{v2}.{v3}.{v4}");

            var clientDeviceInformation = new EasClientDeviceInformation();
            builder.AppendLine($"Manufacturer: {clientDeviceInformation.SystemManufacturer}");
            builder.AppendLine($"ProductName: {clientDeviceInformation.SystemProductName}");

            var con = NetworkInformation.GetInternetConnectionProfile();
            builder.AppendLine($"ConnectionProfile: {con?.ProfileName}");
            builder.AppendLine("**************************************************");
            Debug.WriteLine(builder.ToString());
            var mail = new EmailMessage
            {
                Subject = "ownCloud Universal Windows App - Diagnostic Report",
                Body = builder.ToString()
            };
            var logfile = await ApplicationData.Current.LocalFolder.TryGetItemAsync("log.txt");
            if(logfile != null)
                mail.Attachments.Add(new EmailAttachment("errors.log", (StorageFile)logfile));
            mail.To.Add(new EmailRecipient("adrian.gebhart@live.de", "Adrian Gebhart"));
            await EmailManager.ShowComposeNewEmailAsync(mail);
        }
    }
}
