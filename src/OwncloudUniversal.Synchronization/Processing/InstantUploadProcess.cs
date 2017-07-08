using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using OwncloudUniversal.Model;
using OwncloudUniversal.Synchronization.LocalFileSystem;
using OwncloudUniversal.Synchronization.Model;

namespace OwncloudUniversal.Synchronization.Processing
{
    public class InstantUploadProcess : BackgroundSyncProcess
    {
        private readonly FileSystemAdapter _adapter;
        private List<BaseItem> _itemsToAdd;
        private List<BaseItem> _itemsToUpdate;
        public InstantUploadProcess(FileSystemAdapter fileSystemAdapter, AbstractAdapter targetEntityAdapter, bool isBackgroundTask) : base(fileSystemAdapter, targetEntityAdapter, isBackgroundTask)
        {
            _adapter = fileSystemAdapter;
        }

        protected override async Task GetChanges()
        {
            _itemsToAdd = new List<BaseItem>();
            _itemsToUpdate = new List<BaseItem>();
            foreach (var association in FolderAssociationTableModel.GetDefault().GetAllItems())
            {
                if (association.SupportsInstantUpload)
                {
                    var adds = await _adapter.GetChangesFromChangeTracker(KnownLibraryId.Pictures, association, new List<StorageLibraryChangeType>{StorageLibraryChangeType.Created, StorageLibraryChangeType.MovedIntoLibrary, StorageLibraryChangeType.MovedOrRenamed});
                    _itemsToAdd.AddRange(adds);
                    var updated = await _adapter.GetChangesFromChangeTracker(KnownLibraryId.Pictures, association, new List<StorageLibraryChangeType>{ StorageLibraryChangeType.ContentsChanged, StorageLibraryChangeType.ContentsReplaced });
                    _itemsToUpdate.AddRange(updated);
                    var deletions = await _adapter.GetChangesFromChangeTracker(KnownLibraryId.Pictures, association, new List<StorageLibraryChangeType> { StorageLibraryChangeType.Deleted, StorageLibraryChangeType.MovedOrRenamed, StorageLibraryChangeType.MovedOutOfLibrary });

                    if(adds == null || updated == null || deletions == null)//changetracking was reset in this case
                        return;

                    await _UpdateFileIndexes(association, adds);
                    await _UpdateFileIndexes(association, updated);
                    await ProcessDeletions(deletions);

                    await _adapter.AcceptChangesFromChangeTracker();
                }
            }
        }

        protected override async Task ProcessItems()
        {
            List<dynamic> itemsToUpdate = new List<dynamic>();
            
            if (_itemsToUpdate.Count != 0)
            {
                var links = LinkStatusTableModel.GetDefault().GetAllItems();
                foreach (var baseItem in _itemsToUpdate)
                {
                    var existingItem = ItemTableModel.GetDefault().GetItem(baseItem);
                    if (existingItem == null)
                        _itemsToAdd.Add(baseItem);
                    else
                    {
                        var itemsToProcess = (from link in links
                                .Where(x => x.SourceItemId == existingItem.Id || x.TargetItemId == existingItem.Id)
                                .DefaultIfEmpty()
                            select new { LinkStatus = link, BaseItem = existingItem }).ToList();

                        _itemsToAdd.AddRange((from item in itemsToProcess
                            where item.LinkStatus == null
                            select item.BaseItem).ToList());

                        itemsToUpdate.AddRange((from item in itemsToProcess
                            where item.LinkStatus != null
                            select item).ToList());
                    }
                }
            }
            
            
            await ProcessAdds(_itemsToAdd);
            await ProcessUpdates(itemsToUpdate);
        }
    }
}
