using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwncloudUniversal.Utils
{
    public static class MimetypeIconUtil
    {
        public static string GetIconName(string mimeType)
        {
            if(ExtendedMimeTypes.Count == 0)
                CreateExtendedMimeTypeIconMapping();
            if(SimpleMimeTypes.Count == 0)
                CreateSimpleMimeTypeMapping();

            string name;

            if (ExtendedMimeTypes.TryGetValue(mimeType, out name))
                return name;

            string mainType = "";
            var index = mimeType.IndexOf('/');
            if (index != -1)
                mainType = mimeType.Substring(0, index);
            if (SimpleMimeTypes.TryGetValue(mainType, out name))
                return name;

            return "file.png";
        }

        private static readonly Dictionary<string, string> ExtendedMimeTypes = new Dictionary<string, string>();

        private static void CreateExtendedMimeTypeIconMapping()
        {
            ExtendedMimeTypes.Add("application/coreldraw", "image.png");
            ExtendedMimeTypes.Add("application/epub+zip", "text.png");
            ExtendedMimeTypes.Add("application/font-sfnt", "image.png");
            ExtendedMimeTypes.Add("application/font-woff", "image.png");
            ExtendedMimeTypes.Add("application/illustrator", "image.png");
            ExtendedMimeTypes.Add("application/javascript", "text-code.png");
            ExtendedMimeTypes.Add("application/json", "text-code.png");
            ExtendedMimeTypes.Add("application/msaccess", "file.png");
            ExtendedMimeTypes.Add("application/msexcel", "x-office-spreadsheet.png");
            ExtendedMimeTypes.Add("application/mspowerpoint", "x-office-presentation.png");
            ExtendedMimeTypes.Add("application/msword", "x-office-document.png");
            ExtendedMimeTypes.Add("application/octet-stream", "file.png");
            ExtendedMimeTypes.Add("application/postscript", "image.png");
            ExtendedMimeTypes.Add("application/pdf", "application-pdf.png");
            ExtendedMimeTypes.Add("application/rss+xml", "text-code.png");
            ExtendedMimeTypes.Add("application/rtf", "file.png");
            ExtendedMimeTypes.Add("application/vnd.android.package-archive", "package-x-generic.png");
            ExtendedMimeTypes.Add("application/vnd.ms-excel", "x-office-spreadsheet.png");
            ExtendedMimeTypes.Add("application/vnd.ms-excel.addin.macroEnabled.12", "x-office-spreadsheet.png");
            ExtendedMimeTypes.Add("application/vnd.ms-excel.sheet.binary.macroEnabled.12", "x-office-spreadsheet.png");
            ExtendedMimeTypes.Add("application/vnd.ms-excel.sheet.macroEnabled.12", "x-office-spreadsheet.png");
            ExtendedMimeTypes.Add("application/vnd.ms-excel.template.macroEnabled.12", "x-office-spreadsheet.png");
            ExtendedMimeTypes.Add("application/vnd.ms-fontobject", "image.png");
            ExtendedMimeTypes.Add("application/vnd.ms-powerpoint", "x-office-presentation.png");
            ExtendedMimeTypes.Add("application/vnd.ms-powerpoint.addin.macroEnabled.12", "x-office-presentation.png");
            ExtendedMimeTypes.Add("application/vnd.ms-powerpoint.presentation.macroEnabled.12","x-office-presentation.png");
            ExtendedMimeTypes.Add("application/vnd.ms-powerpoint.slideshow.macroEnabled.12","x-office-presentation.png");
            ExtendedMimeTypes.Add("application/vnd.ms-powerpoint.template.macroEnabled.12", "x-office-presentation.png");
            ExtendedMimeTypes.Add("application/vnd.ms-word.document.macroEnabled.12", "x-office-document.png");
            ExtendedMimeTypes.Add("application/vnd.ms-word.template.macroEnabled.12", "x-office-document.png");
            ExtendedMimeTypes.Add("application/vnd.oasis.opendocument.presentation", "x-office-presentation.png");
            ExtendedMimeTypes.Add("application/vnd.oasis.opendocument.presentation-template","x-office-presentation.png");
            ExtendedMimeTypes.Add("application/vnd.oasis.opendocument.spreadsheet", "x-office-spreadsheet.png");
            ExtendedMimeTypes.Add("application/vnd.oasis.opendocument.spreadsheet-template", "x-office-spreadsheet.png");
            ExtendedMimeTypes.Add("application/vnd.oasis.opendocument.text", "x-office-document.png");
            ExtendedMimeTypes.Add("application/vnd.oasis.opendocument.text-master", "x-office-document.png");
            ExtendedMimeTypes.Add("application/vnd.oasis.opendocument.text-template", "x-office-document.png");
            ExtendedMimeTypes.Add("application/vnd.oasis.opendocument.text-web", "x-office-document.png");
            ExtendedMimeTypes.Add("application/vnd.openxmlformats-officedocument.presentationml.presentation","x-office-presentation.png");
            ExtendedMimeTypes.Add("application/vnd.openxmlformats-officedocument.presentationml.slideshow","x-office-presentation.png");
            ExtendedMimeTypes.Add("application/vnd.openxmlformats-officedocument.presentationml.template","x-office-presentation.png");
            ExtendedMimeTypes.Add("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet","x-office-spreadsheet.png");
            ExtendedMimeTypes.Add("application/vnd.openxmlformats-officedocument.spreadsheetml.template","x-office-spreadsheet.png");
            ExtendedMimeTypes.Add("application/vnd.openxmlformats-officedocument.wordprocessingml.document","x-office-document.png");
            ExtendedMimeTypes.Add("application/vnd.openxmlformats-officedocument.wordprocessingml.template","x-office-document.png");
            ExtendedMimeTypes.Add("application/x-7z-compressed", "package-x-generic.png");
            ExtendedMimeTypes.Add("application/x-bin", "application.png");
            ExtendedMimeTypes.Add("application/x-cbr", "text.png");
            ExtendedMimeTypes.Add("application/x-compressed", "package-x-generic.png");
            ExtendedMimeTypes.Add("application/x-dcraw", "image.png");
            ExtendedMimeTypes.Add("application/x-deb", "package-x-generic.png");
            ExtendedMimeTypes.Add("application/x-font", "image.png");
            ExtendedMimeTypes.Add("application/x-gimp", "image.png");
            ExtendedMimeTypes.Add("application/x-gzip", "package-x-generic.png");
            ExtendedMimeTypes.Add("application/x-ms-dos-executable", "application.png");
            ExtendedMimeTypes.Add("application/x-msi", "application.png");
            ExtendedMimeTypes.Add("application/x-iwork-numbers-sffnumbers", "x-office-spreadsheet.png");
            ExtendedMimeTypes.Add("application/x-iwork-keynote-sffkey", "x-office-presentation.png");
            ExtendedMimeTypes.Add("application/x-iwork-pages-sffpages", "x-office-document.png");
            ExtendedMimeTypes.Add("application/x-perl", "text-code.png");
            ExtendedMimeTypes.Add("application/x-photoshop", "image.png");
            ExtendedMimeTypes.Add("application/x-php", "text-code.png");
            ExtendedMimeTypes.Add("application/x-rar-compressed", "package-x-generic.png");
            ExtendedMimeTypes.Add("application/x-shockwave-flash", "application.png");
            ExtendedMimeTypes.Add("application/x-tar", "package-x-generic.png");
            ExtendedMimeTypes.Add("application/x-tex", "text.png");
            ExtendedMimeTypes.Add("application/xml", "text.png");
            ExtendedMimeTypes.Add("application/yaml", "text-code.png");
            ExtendedMimeTypes.Add("application/zip", "package-x-generic.png");
            ExtendedMimeTypes.Add("database", "file.png");
            ExtendedMimeTypes.Add("httpd/unix-directory", "folder.png");
            ExtendedMimeTypes.Add("image/svg+xml", "image.png");
            ExtendedMimeTypes.Add("image/vector", "image.png");
            ExtendedMimeTypes.Add("text/calendar", "calendar");
            ExtendedMimeTypes.Add("text/css", "text-code.png");
            ExtendedMimeTypes.Add("text/csv", "x-office-spreadsheet.png");
            ExtendedMimeTypes.Add("text/html", "text-code.png");
            ExtendedMimeTypes.Add("text/vcard", "vcard");
            ExtendedMimeTypes.Add("text/x-c", "text-code.png");
            ExtendedMimeTypes.Add("text/x-c++src", "text-code.png");
            ExtendedMimeTypes.Add("text/x-h", "text-code.png");
            ExtendedMimeTypes.Add("text/x-python", "text-code.png");
            ExtendedMimeTypes.Add("text/x-shellscript", "text-code.png");
            ExtendedMimeTypes.Add("text/directory", "folder.png");
            ExtendedMimeTypes.Add("web", "text-code.png");
            ExtendedMimeTypes.Add("DIR", "folder.png");
        }

        private static readonly Dictionary<string, string> SimpleMimeTypes = new Dictionary<string, string>();

        private static void CreateSimpleMimeTypeMapping()
        {
            SimpleMimeTypes.Add("audio", "audio.png");
            SimpleMimeTypes.Add("database", "file.png");
            SimpleMimeTypes.Add("httpd", "package-x-generic.png");
            SimpleMimeTypes.Add("image", "image.png");
            SimpleMimeTypes.Add("text", "text.png");
            SimpleMimeTypes.Add("video", "video.png");
            SimpleMimeTypes.Add("web", "text-code.png");
        }
    }
}
