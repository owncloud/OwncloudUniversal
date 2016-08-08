using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using owncloud_universal.Model;
using owncloud_universal.WebDav;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Search;
using Windows.UI.Popups;
using Windows.Storage.FileProperties;
using System.Diagnostics;

namespace owncloud_universal
{
    class ProcessingManager
    {
        public async Task Run()
        {
            var items = FolderAssociationTableModel.GetDefault().GetAllItems();
            foreach (var item in items)
            {
                await UpdateItemInfos(item);
                var upload = await GetDataToUpload(item);
                await UploadItems(item, upload);

                var download = await GetDataToDownload(item);
                await DownloadItems(item, download);
            }
        }

        


        private async Task UpdateItemInfos(FolderAssociation association)
        {
            StorageFolder localFolder = await StorageFolder.GetFolderFromPathAsync(association.LocalFolder.Path);
            await ScanLocalFolder(localFolder, association.Id);
            await ScanRemoteFolder(association.RemoteFolder, association.Id);
        }


    }
}