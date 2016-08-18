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
using owncloud_universal.LocalFileSystem;
using SQLitePCL;

namespace owncloud_universal
{
    class ProcessingManager
    {
        private WebDavAdapter _webDavAdapter;
        private FileSystemAdapter _fileSystemAdapter;
        private List<AbstractItem> itemIndex;
        private List<LinkStatus> linkList;
        public async Task Run()
        {
            _webDavAdapter = new WebDavAdapter();
            _fileSystemAdapter = new FileSystemAdapter();
            var items = FolderAssociationTableModel.GetDefault().GetAllItems();
            foreach (FolderAssociation item in items)
            {
                itemIndex = await _webDavAdapter.GetAllItems(item);
                itemIndex.AddRange(await _fileSystemAdapter.GetAllItems(item));
                _UpdateFileIndexes(item);
                var model = LinkStatusTableModel.GetDefault();
                linkList = model.GetAllItems(item).ToList();

                foreach (var i in itemIndex)
                {
                    try
                    {
                        await _Process(i);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                }

                //var inserts = GetInserts();
                //var updates = GetUpdates();
                //var deletes = GetDeletes();

                //await DoInserts(inserts);
                //await DoUpdates(updates);
                //await DoDeletes(deletes);
            }
        }  
        
        private async Task _Process(AbstractItem item)
        {
            var link = linkList.Where(x => x.SourceItemId == item.Id || x.TargetItemId == item.Id).FirstOrDefault();
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
                targetItem = await _webDavAdapter.AddItem(item);
            }
            else
            {
                targetItem = await _fileSystemAdapter.AddItem(item);
            }
            return targetItem;

        }

        private async Task<AbstractItem> Update(AbstractItem item)
        {
            AbstractItem result = null;
            if (item.GetType() == typeof(LocalItem))
            {
                result = await _webDavAdapter.UpdateItem(item);
            }
            else if(item.GetType() == typeof(LocalItem))
            {
                result = await _fileSystemAdapter.UpdateItem(item);
            }
            return result;
        }


        private void _UpdateFileIndexes(FolderAssociation association)
        {

            var itemTableModel = AbstractItemTableModel.GetDefault();

            for (int i = 0; i < itemIndex.Count; i++)
            {
                itemIndex[i].Association = association;
                var foundItem = itemTableModel.GetItem(itemIndex[i]);
                if (foundItem == null)
                {
                    itemTableModel.InsertItem(itemIndex[i]);
                    itemIndex[i] = itemTableModel.GetLastInsertItem();
                }
                else
                {
                    itemTableModel.UpdateItem(itemIndex[i], foundItem.Id);
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
            var link = linkList.Where(x => x.SourceItemId == sourceItem.Id || x.TargetItemId == targetItem.Id).First();
            link.ChangeNumber = sourceItem.ChangeNumber;
            LinkStatusTableModel.GetDefault().UpdateItem(link, link.Id);
        }        
    }
}