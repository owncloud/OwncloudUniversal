using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.System.Power;
using Windows.UI.Notifications;
using OwncloudUniversal.Model;
using OwncloudUniversal.Shared.Model;

namespace OwncloudUniversal.Shared.Synchronisation
{
    public class BackgroundSyncProcess
    {       
        private List<AbstractItem> _itemIndex;
        private List<LinkStatus> _linkList;
        private int _uploadCount;
        private int _downloadCount;
        private int _deletedCount;
        private bool _errorsOccured;

        public readonly ExecutionContext ExecutionContext;
        private readonly AbstractAdapter _sourceEntityAdapter;
        private readonly AbstractAdapter _targetEntityAdapter;
        private readonly bool _isBackgroundTask;

        public BackgroundSyncProcess(AbstractAdapter sourceEntityAdapter, AbstractAdapter targetEntityAdapter, bool isBackgroundTask)
        {
            _sourceEntityAdapter = sourceEntityAdapter;
            _targetEntityAdapter = targetEntityAdapter;
            _isBackgroundTask = isBackgroundTask;
            ExecutionContext = new ExecutionContext();
            _errorsOccured = false;

        }

        public async Task Run()
        {
            var watch = Stopwatch.StartNew();
            SQLite.SQLiteClient.Init();
            var associations = FolderAssociationTableModel.GetDefault().GetAllItems();
            foreach (FolderAssociation association in associations)
            {
                await LogHelper.Write($"Scanning {association.LocalFolderPath} and {association.RemoteFolderFolderPath} BackgroundTask: {_isBackgroundTask}");
                if (watch.Elapsed.Minutes >= 9)
                    break;
                ExecutionContext.Status = ExecutionStatus.Scanning;
                _itemIndex = await _targetEntityAdapter.GetUpdatedItems(association);
                _itemIndex.AddRange(await _sourceEntityAdapter.GetUpdatedItems(association));
                _UpdateFileIndexes(association);

                var deleted = await _targetEntityAdapter.GetDeletedItemsAsync(association);
                deleted.AddRange(await _sourceEntityAdapter.GetDeletedItemsAsync(association));
                DeleteFromIndex(deleted);
                _deletedCount = deleted.Count;
                await LogHelper.Write($"Updating: {_itemIndex.Count} Deleting: {deleted.Count} BackgroundTask: {_isBackgroundTask}");
            }
            await LogHelper.Write($"Starting Sync.. BackgroundTask: {_isBackgroundTask}");
            _itemIndex = AbstractItemTableModel.GetDefault().GetAllItems().ToList();
            _linkList = LinkStatusTableModel.GetDefault().GetAllItems().ToList();
            ExecutionContext.Status = ExecutionStatus.Active;
            ExecutionContext.TotalFileCount = _itemIndex.Count;
            int index = 1;
            foreach (var item in _itemIndex)
            {
                try
                {
                    ExecutionContext.CurrentFileNumber = index++;
                    ExecutionContext.CurrentFileName = item.EntityId;
                    if(ExecutionContext.Status == ExecutionStatus.Stopped)
                        break;
                    await _Process(item);
                }
                catch (Exception e)
                {
                    _errorsOccured = true;
                    ToastHelper.SendToast(string.Format("Message: {0}, EntitityId: {1}", e.Message, item.EntityId));
                    await LogHelper.Write(string.Format("Message: {0}, EntitityId: {1}", e.Message, item.EntityId));
                }
                //we have 10 Minutes in total for each background task cycle
                //after 10 minutes windows will terminate the task
                //so after 9 minutes we stop the sync and just wait for the next cycle
                if (watch.Elapsed.Minutes >= 9 && _isBackgroundTask)
                {
                    await LogHelper.Write("Stopping sync-cycle. Please wait for the next cycle to complete the sync");
                    break;
                }
            }

            await LogHelper.Write($"Finished synchronization cycle. Duration: {watch.Elapsed} BackgroundTask: {_isBackgroundTask}");
            ToastHelper.SendToast(_isBackgroundTask
                ? $"BackgroundTask: {_uploadCount} Files Uploaded, {_downloadCount} Files Downloaded {_deletedCount} Files Deleted. Duration: {watch.Elapsed}"
                : $"ManualSync: {_uploadCount} Files Uploaded, {_downloadCount} Files Downloaded {_deletedCount} Files Deleted. Duration: {watch.Elapsed}");
            watch.Stop();
            if(!_errorsOccured)
                Configuration.LastSync = DateTime.UtcNow.ToString("yyyy\\-MM\\-dd\\THH\\:mm\\:ss\\Z");
            ExecutionContext.Status = ExecutionStatus.Finished;
        }      

        private async Task _Process(AbstractItem item)
        {
            //skip files bigger than 50MB, these will have to be synced manually
            //otherwise the upload/download could take too long and task would be terminated
            //TODO make this configurable
            if (item.Size > (50 * 1024 * 1024) & _isBackgroundTask)
            {
                item.SyncPostponed = true;
                AbstractItemTableModel.GetDefault().UpdateItem(item, item.Id);
                return;
            }
            var link = _linkList.FirstOrDefault(x => (x.SourceItemId == item.Id || x.TargetItemId == item.Id) && x.AssociationId == item.Association.Id);
            if (link == null)
            {
                //es ist noch kein link vorhanden, also ein neues Item
                var result = await Insert(item);
                AfterInsert(item, result);
            }
            if(link  != null)
            {
                if (item.ChangeNumber > link.ChangeNumber)
                {
                    var result = await Update(item);
                    AfterUpdate(item, result);
                }
            }
        }

        private async Task<AbstractItem> Insert(AbstractItem item)
        {

            AbstractItem targetItem = null;
            item.SyncPostponed = false;
            if (item.AdapterType == _targetEntityAdapter.GetType())
            {
                targetItem = await _sourceEntityAdapter.AddItem(item);
                if(!item.IsCollection)
                    _downloadCount++;
            }
            else if (item.AdapterType == _sourceEntityAdapter.GetType())
            {
                targetItem = await _targetEntityAdapter.AddItem(item);
                if(!item.IsCollection)
                    _uploadCount++;
            }
            return targetItem;

        }

        private async Task<AbstractItem> Update(AbstractItem item)
        {
            AbstractItem result = null;
            item.SyncPostponed = false;
            if (item.AdapterType == _targetEntityAdapter.GetType())
            {
                result = await _sourceEntityAdapter.UpdateItem(item);
                if(!item.IsCollection)
                    _downloadCount++;
            }
            else if(item.AdapterType == _sourceEntityAdapter.GetType())
            {
                result = await _targetEntityAdapter.UpdateItem(item);
                if(!item.IsCollection)
                    _uploadCount++;
            }
            return result;
        }

        private void _UpdateFileIndexes(FolderAssociation association)
        {
            var itemTableModel = AbstractItemTableModel.GetDefault();

            foreach (AbstractItem t in _itemIndex)
            {
                t.Association = association;
                var foundItem = itemTableModel.GetItem(t);
                if (foundItem == null)
                {
                    itemTableModel.InsertItem(t);
                    t.Id = itemTableModel.GetLastInsertItem().Id;
                }
                else
                {
                    if (foundItem.ChangeKey != t.ChangeKey)
                    {
                        t.ChangeNumber = foundItem.ChangeNumber + 1;
                        itemTableModel.UpdateItem(t, foundItem.Id);
                    }
                    t.Id = foundItem.Id;

                }
            }
        }

        private void DeleteFromIndex(List<AbstractItem> itemsToDelete)
        {
            foreach (var abstractItem in itemsToDelete)
            {
                AbstractItemTableModel.GetDefault().DeleteItem(abstractItem.Id);
            }
        }

        private void AfterInsert(AbstractItem sourceItem, AbstractItem targetItem)
        {
            if (targetItem.Association == null)
                targetItem.Association = sourceItem.Association;
            //check if item with same path already exists
            var existingItem = AbstractItemTableModel.GetDefault().GetItem(targetItem);
            if (existingItem != null)
            {
                AbstractItemTableModel.GetDefault().UpdateItem(targetItem, existingItem.Id);
                targetItem.Id = existingItem.Id;
            }
            else
            {
                AbstractItemTableModel.GetDefault().InsertItem(targetItem);
                targetItem = AbstractItemTableModel.GetDefault().GetLastInsertItem();
            }

            LinkStatus link = new LinkStatus(sourceItem, targetItem);
            LinkStatusTableModel.GetDefault().InsertItem(link);
        }

        private void AfterUpdate(AbstractItem sourceItem, AbstractItem targetItem)
        {
            if (targetItem.Association == null)
                targetItem.Association = sourceItem.Association;
            targetItem.ChangeNumber = sourceItem.ChangeNumber;
            AbstractItemTableModel.GetDefault().UpdateItem(sourceItem, sourceItem.Id);
            AbstractItemTableModel.GetDefault().UpdateItem(targetItem, targetItem.Id);
            var link = _linkList.FirstOrDefault(x => (x.SourceItemId == sourceItem.Id || x.TargetItemId == sourceItem.Id) && x.AssociationId == sourceItem.Association.Id);
            link.ChangeNumber = sourceItem.ChangeNumber;
            LinkStatusTableModel.GetDefault().UpdateItem(link, link.Id);
        }        
    }
}