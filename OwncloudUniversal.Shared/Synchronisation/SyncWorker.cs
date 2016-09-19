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

        public ProcessingManager(AbstractAdapter sourceEntityAdapter, AbstractAdapter targetEntityAdapter)
        {
            _sourceEntityAdapter = sourceEntityAdapter;
            _targetEntityAdapter = targetEntityAdapter;
            logHelper = new LogHelper();
        }

        public async Task Run()
        {
            var watch = Stopwatch.StartNew();
            logHelper.Write("Starting Sync");
            SQLite.SQLiteClient.Init();
            var items = FolderAssociationTableModel.GetDefault().GetAllItems();
            foreach (FolderAssociation item in items)
            {
                if (watch.Elapsed.Minutes >= 9)
                    break;

                _itemIndex = await _targetEntityAdapter.GetAllItems(item);
                _itemIndex.AddRange(await _sourceEntityAdapter.GetAllItems(item));
                _UpdateFileIndexes(item);
                var model = LinkStatusTableModel.GetDefault();
                _linkList = model.GetAllItems(item).ToList();

                foreach (var i in _itemIndex)
                {
                    try
                    {
                        //skip files bigger than 50MB, these will have to be synced manually
                        //otherwise the upload/download could take too long and task would be terminated
                        //TODO make this configurable
                        if (i.Size > (50 * 1024 * 1024))
                        {
                            continue;
                        }
                        await _Process(i);
                    }
                    catch (Exception e)
                    {
                        ToastHelper.SendToast(string.Format("Message: {0}, EntitityId: {1}", e.Message, i.EntityId));
                        logHelper.Write(string.Format("Message: {0}, EntitityId: {1}", e.Message, i.EntityId));
                    }
                    //we have 10 Minutes in total for each background task cycle
                    //after 10 minutes windows will terminate the task
                    //so after 9 minutes we stop the sync and just wait for the next cycle
                    if (watch.Elapsed.Minutes >= 9)
                        break;
                }
            }
            logHelper.Write("Finished synchronization cycle");
            ToastHelper.SendToast($"{_uploadCount} Files Uploaded, {_downloadCount} Files Downloaded");
            watch.Stop();

        }

        

        private async Task _Process(AbstractItem item)
        {
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
            if (item is LocalItem)
            {
                targetItem = await _targetEntityAdapter.AddItem(item);
                _uploadCount++;
            }
            else if (item is RemoteItem)
            {
                targetItem = await _sourceEntityAdapter.AddItem(item);
                _downloadCount++;
            }
            return targetItem;

        }

        private async Task<AbstractItem> Update(AbstractItem item)
        {
            AbstractItem result = null;
            if (item is LocalItem)
            {
                result = await _targetEntityAdapter.UpdateItem(item);
                _uploadCount++;
            }
            else if(item is RemoteItem)
            {
                result = await _sourceEntityAdapter.UpdateItem(item);
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