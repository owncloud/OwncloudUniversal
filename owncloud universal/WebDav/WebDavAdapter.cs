using owncloud_universal.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace owncloud_universal.WebDav
{
    class WebDavAdapter
    {
        private async Task ScanRemoteFolder(RemoteItem remoteFolder, long associationId)
        {
            List<RemoteItem> items = await ConnectionManager.GetFolder(remoteFolder.DavItem.Href);
            foreach (RemoteItem item in items)
            {
                RemoteItem ri = new RemoteItem(item.DavItem);
                ri.FolderId = associationId;
                var foundItems = RemoteItemTableModel.GetDefault().SelectByPath(ri.DavItem.Href, ri.FolderId);
                if (foundItems.Count == 0)
                {
                    RemoteItemTableModel.GetDefault().InsertItem(ri);
                    if (!ri.DavItem.IsCollection) continue;
                    await ScanRemoteFolder(item, associationId);
                    continue;
                }

                foreach (var foundItem in foundItems)
                {
                    if (foundItem.DavItem.Etag != ri.DavItem.Etag)
                    {
                        RemoteItemTableModel.GetDefault().UpdateItem(ri, foundItem.Id);
                        Debug.Write(string.Format("Updating Database {0}", foundItem.Id));
                        if (!ri.DavItem.IsCollection) continue;
                        await ScanRemoteFolder(item, associationId);
                    }
                }

            }
        }

        private async Task<List<RemoteItem>> GetDataToDownload(FolderAssociation association)
        {
            var result = new List<RemoteItem>();
            await CheckRemoteFolderRecursive(association, result);
            return result;
        }



        private async Task CheckRemoteFolderRecursive(FolderAssociation association, List<RemoteItem> result)
        {
            List<RemoteItem> items = await ConnectionManager.GetFolder(association.RemoteFolder.DavItem.Href);
            var inserts = RemoteItemTableModel.GetDefault().GetInserts(association.Id);
            result.AddRange(inserts);
            foreach (RemoteItem item in items)
            {
                if (item.DavItem.IsCollection)
                    await CheckRemoteFolderRecursive(association, result);

                var foundItems = RemoteItemTableModel.GetDefault().SelectByPath(item.DavItem.Href, item.FolderId);
                if (foundItems.Count == 0)
                    result.Add(item);
                else foreach (var foundItem in foundItems)
                    {
                        if (foundItem.DavItem.Etag != item.DavItem.Etag)
                            result.Add(item);
                    }
            }
        }
    }
}
