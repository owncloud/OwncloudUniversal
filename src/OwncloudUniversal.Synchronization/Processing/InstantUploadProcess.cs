using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
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

                    await _UpdateFileIndexes(association, adds);
                    await _UpdateFileIndexes(association, updated);
                    await ProcessDeletions(deletions);

                    await _adapter.AcceptChangesFromChangeTracker(KnownLibraryId.Pictures);
                }
            }
        }

        protected override async Task ProcessItems()
        {
            await ProcessAdds(_itemsToAdd);
            await ProcessUpdates(_itemsToUpdate);
        }
    }
}
