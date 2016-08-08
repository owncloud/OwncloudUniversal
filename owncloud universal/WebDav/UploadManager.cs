using owncloud_universal.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace owncloud_universal
{
    class UploadManager
    {
        private async Task UploadItems(FolderAssociation association, List<LocalItem> items)
        {
            foreach (var item in items)
            {
                if (item.IsCollection)
                {
                    string folderPath = BuildRemotePath(association, item.Path);
                }
                StorageFile file = await StorageFile.GetFileFromPathAsync(item.Path);
                var stream = await file.OpenStreamForReadAsync();
                await ConnectionManager.Upload(BuildRemotePath(association, file.Path), stream, file.DisplayName);
                var remoteItem = ConnectionManager.GetFolder(BuildRemotePath(association, file.Path));

                LocalItemTableModel.GetDefault().UpdateItem(item, item.Id);
            }
        }

        private string BuildRemotePath(FolderAssociation association, string path)
        {
            Uri baseUri = new Uri(association.LocalFolder.Path);
            Uri fileUri = new Uri(path);
            Uri relativeUri = baseUri.MakeRelativeUri(fileUri);
            string uri = relativeUri.ToString();
            var relativeString = uri.Substring(uri.IndexOf('/') + 1);
            return association.RemoteFolder.DavItem.Href + relativeString;
        }
    }
}
