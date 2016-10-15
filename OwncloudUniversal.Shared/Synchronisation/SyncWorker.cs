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
    public class ProcessingManager
    {       
        private List<AbstractItem> _itemIndex;
        private List<LinkStatus> _linkList;
        private int _uploadCount;
        private int _downloadCount;

        private readonly AbstractAdapter _sourceEntityAdapter;
        private readonly AbstractAdapter _targetEntityAdapter;
        private LogHelper logHelper;
        private bool _isBackgroundTask;

        public ProcessingManager(AbstractAdapter sourceEntityAdapter, AbstractAdapter targetEntityAdapter, bool isBackgroundTask)
        {
            _sourceEntityAdapter = sourceEntityAdapter;
            _targetEntityAdapter = targetEntityAdapter;
            _isBackgroundTask = isBackgroundTask;
            logHelper = new LogHelper();
            
        }

        public async Task Run()
        {
            var watch = Stopwatch.StartNew();
            await logHelper.Write("**************************************");
            if (_isBackgroundTask)
                await logHelper.Write("starting background sync");
            else
                await logHelper.Write("starting manual sync");
            SQLite.SQLiteClient.Init();
            var items = FolderAssociationTableModel.GetDefault().GetAllItems();
            foreach (FolderAssociation item in items)
            {
                await logHelper.Write($"Syncing {item.LocalFolderPath} with {item.RemoteFolderFolderPath}");
                if (watch.Elapsed.Minutes >= 9)
                    break;
                await logHelper.Write("scanning remote items");
                _itemIndex = await _targetEntityAdapter.GetAllItems(item);
                await logHelper.Write("scanning local items");
                _itemIndex.AddRange(await _sourceEntityAdapter.GetAllItems(item));
                await logHelper.Write("updating database");
                _UpdateFileIndexes(item);
                var model = LinkStatusTableModel.GetDefault();
                _linkList = model.GetAllItems(item).ToList();
                await logHelper.Write("processing items");
                foreach (var i in _itemIndex)
                {
                    try
                    {
                        await _Process(i);
                    }
                    catch (Exception e)
                    {
                        ToastHelper.SendToast(string.Format("Message: {0}, EntitityId: {1}", e.Message, i.EntityId));
                        await logHelper.Write(string.Format("Message: {0}, EntitityId: {1}, \r\n{2}", e.Message, i.EntityId, e.StackTrace));
                    }
                    //we have 10 Minutes in total for each background task cycle
                    //after 10 minutes windows will terminate the task
                    //so after 9 minutes we stop the sync and just wait for the next cycle
                    if (!_isBackgroundTask || watch.Elapsed.Minutes >= 9) continue;
                    await logHelper.Write("Stopping sync-cycle. Please wait for the next cycle to complete the sync");
                    break;
                }
            }
            await logHelper.Write("Finished synchronization cycle");
            ToastHelper.SendToast($"{_uploadCount} Files Uploaded, {_downloadCount} Files Downloaded");
            watch.Stop();
            Configuration.LastSync = DateTime.UtcNow.ToString("yyyy\\-MM\\-dd\\THH\\:mm\\:ss\\Z");
        }

        

        private async Task _Process(AbstractItem item)
        {
            //skip files bigger than 50MB, these will have to be synced manually
            //otherwise the upload/download could take too long and task would be terminated
            //TODO make this configurable
            if (_isBackgroundTask && item.Size > (50 * 1024 * 1024))
            {
                item.SyncPostponed = true;
                return;
            }
            var link = _linkList.FirstOrDefault(x => x.SourceItemId == item.Id || x.TargetItemId == item.Id);
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
            if (item is LocalItem)
            {
                targetItem = await _targetEntityAdapter.AddItem(item);
                if(!item.IsCollection)
                    _uploadCount++;
            }
            else if (item is RemoteItem)
            {
                targetItem = await _sourceEntityAdapter.AddItem(item);
                if(!item.IsCollection)
                    _downloadCount++;
            }
            return targetItem;

        }

        private async Task<AbstractItem> Update(AbstractItem item)
        {
            AbstractItem result = null;
            item.SyncPostponed = false;
            if (item is LocalItem)
            {
                result = await _targetEntityAdapter.UpdateItem(item);
                if(!item.IsCollection)
                    _uploadCount++;
            }
            else if(item is RemoteItem)
            {
                result = await _sourceEntityAdapter.UpdateItem(item);
                if(!item.IsCollection)
                    _downloadCount++;
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

        private void AfterInsert(AbstractItem sourceItem, AbstractItem targetItem)
        {
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
            targetItem.ChangeNumber = sourceItem.ChangeNumber;
            AbstractItemTableModel.GetDefault().UpdateItem(sourceItem, sourceItem.Id);
            AbstractItemTableModel.GetDefault().UpdateItem(targetItem, targetItem.Id);
            var link = _linkList.First(x => x.SourceItemId == sourceItem.Id || x.TargetItemId == targetItem.Id);
            link.ChangeNumber = sourceItem.ChangeNumber;
            LinkStatusTableModel.GetDefault().UpdateItem(link, link.Id);
        }        
    }
}