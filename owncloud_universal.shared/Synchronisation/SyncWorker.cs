using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.System.Power;
using OwncloudUniversal.Model;
using OwncloudUniversal.Shared.Model;

namespace OwncloudUniversal.Shared.Synchronisation
{
    public class ProcessingManager
    {       
        private List<AbstractItem> _itemIndex;
        private List<LinkStatus> _linkList;

        private readonly AbstractAdapter _sourceEntityAdapter;
        private readonly AbstractAdapter _targetEntityAdapter;

        public ProcessingManager(AbstractAdapter sourceEntityAdapter, AbstractAdapter targetEntityAdapter)
        {
            _sourceEntityAdapter = sourceEntityAdapter;
            _targetEntityAdapter = targetEntityAdapter;
        }

        public async Task Run()
        {
            SQLite.SQLiteClient.Init();
            var items = FolderAssociationTableModel.GetDefault().GetAllItems();
            foreach (FolderAssociation item in items)
            {
                _itemIndex = await _targetEntityAdapter.GetAllItems(item);
                _itemIndex.AddRange(await _sourceEntityAdapter.GetAllItems(item));
                _UpdateFileIndexes(item);
                var model = LinkStatusTableModel.GetDefault();
                _linkList = model.GetAllItems(item).ToList();

                foreach (var i in _itemIndex)
                {
                    try
                    {
                        await _Process(i);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(string.Format("Message: {0}, EntitityId: {1}", e.Message, i.EntityId));
                    }
                }
            }
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
            if (item.GetType() == typeof(LocalItem))
            {
                targetItem = await _targetEntityAdapter.AddItem(item);
            }
            else if (item.GetType() == typeof(RemoteItem))
            {
                targetItem = await _sourceEntityAdapter.AddItem(item);
            }
            return targetItem;

        }

        private async Task<AbstractItem> Update(AbstractItem item)
        {
            AbstractItem result = null;
            if (item.GetType() == typeof(LocalItem))
            {
                result = await _targetEntityAdapter.UpdateItem(item);
            }
            else if(item.GetType() == typeof(LocalItem))
            {
                result = await _sourceEntityAdapter.UpdateItem(item);
            }
            return result;
        }


        private void _UpdateFileIndexes(FolderAssociation association)
        {

            var itemTableModel = AbstractItemTableModel.GetDefault();

            for (int i = 0; i < _itemIndex.Count; i++)
            {
                _itemIndex[i].Association = association;
                var foundItem = itemTableModel.GetItem(_itemIndex[i]);
                if (foundItem == null)
                {
                    itemTableModel.InsertItem(_itemIndex[i]);
                    _itemIndex[i].Id = itemTableModel.GetLastInsertItem().Id;
                }
                else
                {
                    itemTableModel.UpdateItem(_itemIndex[i], foundItem.Id);
                }
            }

            //TODO delete old items??
        }   

        private void AfterInsert(AbstractItem sourceItem, AbstractItem targetItem)
        {
            AbstractItemTableModel.GetDefault().InsertItem(targetItem);
            targetItem = AbstractItemTableModel.GetDefault().GetLastInsertItem();
            LinkStatus link = new LinkStatus(sourceItem, targetItem);
            LinkStatusTableModel.GetDefault().InsertItem(link);
        }

        private void AfterUpdate(AbstractItem sourceItem, AbstractItem targetItem)
        {
            sourceItem.ChangeNumber++;
            targetItem.ChangeNumber++;
            AbstractItemTableModel.GetDefault().UpdateItem(sourceItem, sourceItem.Id);
            AbstractItemTableModel.GetDefault().UpdateItem(targetItem, targetItem.Id);
            var link = _linkList.First(x => x.SourceItemId == sourceItem.Id || x.TargetItemId == targetItem.Id);
            link.ChangeNumber = sourceItem.ChangeNumber;
            LinkStatusTableModel.GetDefault().UpdateItem(link, link.Id);
        }        
    }
}