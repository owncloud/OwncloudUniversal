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
        private List<AbstractItem> allItems;
        private List<LinkStatus> linkList;
        public async Task Run()
        {
            _webDavAdapter = new WebDavAdapter();
            _fileSystemAdapter = new FileSystemAdapter();
            var items = FolderAssociationTableModel.GetDefault().GetAllItems();
            foreach (FolderAssociation item in items)
            {
                allItems = await _webDavAdapter.GetAllItems(item);
                allItems.AddRange(await _fileSystemAdapter.GetAllItems(item));
                _UpdateFileIndexes(item);
                var model = LinkStatusTableModel.GetDefault();
                linkList = model.GetAllItems(item).ToList();

                var inserts = GetInserts();
                var updates = GetUpdates();
                var deletes = GetDeletes();

                DoInserts(inserts);
                DoUpdates(updates);
                DoDeletes(deletes);
            }
        }  
        
        private async void DoInserts(List<AbstractItem> items)
        {
            //add the folders
            foreach (var item in items)
            {
                AbstractItem targetItem = null;
                if (item.IsCollection)
                {
                    if (item.GetType() == typeof(LocalItem))
                    {
                        targetItem = await _webDavAdapter.AddItem(item);
                    }
                    else
                    {
                        targetItem = await _fileSystemAdapter.AddItem(item);
                    }
                }
                //creat link and increase changenum
            }

            //then the files
            foreach (var item in items)
            {
                if(item.GetType() == typeof(LocalItem))
                {
                    await _webDavAdapter.AddItem(item);
                }
                else
                {
                    await _fileSystemAdapter.AddItem(item);
                }
                //creat link and increase changenum
            }
        }

        private async void DoUpdates(List<AbstractItem> items)
        {
            foreach (var item in items)
            {
                if (item.GetType() == typeof(LocalItem))
                {
                    await _webDavAdapter.UpdateItem(item);
                }
                else
                {
                    await _fileSystemAdapter.UpdateItem(item);
                }
                //creat link and increase changenum
            }
        }

        private async void DoDeletes(List<AbstractItem> items)
        {
            //delete files
            foreach (var item in items)
            {
                if(!item.IsCollection)
                {
                    if (item.GetType() == typeof(LocalItem))
                    {
                        await _webDavAdapter.DeleteItem(item);
                    }
                    else
                    {
                        await _fileSystemAdapter.DeleteItem(item);
                    }
                }
            }
            //and then folders
            foreach (var item in items)
            {
                if (item.IsCollection)
                {
                    if (item.GetType() == typeof(LocalItem))
                    {
                        await _webDavAdapter.DeleteItem(item);
                    }
                    else
                    {
                        await _fileSystemAdapter.DeleteItem(item);
                    }
                }
            }
            //delete link and item
        }

        private void _UpdateFileIndexes(FolderAssociation association)
        {

            var itemTableModel = AbstractItemTableModel.GetDefault();
            
            foreach (var item in allItems)
            {
                item.Association = association;
                var foundItem = itemTableModel.GetItem(item);
                if (foundItem == null)
                {
                    itemTableModel.InsertItem(item);
                }
                else
                {
                    itemTableModel.UpdateItem(item, foundItem.Id);
                }
            }

            //TODO delete old items??
        }   

        private List<AbstractItem> GetInserts()
        {
            var result = new List<AbstractItem>();
            foreach (var item in allItems)
            {
                var links = linkList.Where(x => x.SourceItemId == item.Id || x.TargetItemId == item.Id).ToList();
                if(links.Count == 0)
                {
                    //es ist noch kein link vorhanden, also ein neues Item
                    result.Add(item);
                }
            }
            return result;
        }

        private List<AbstractItem> GetUpdates()
        {
            var result = new List<AbstractItem>();
            foreach (var item in allItems)
            {
                //wenn die chngenum vom link kleiner ist als die vom item muss es geupdated werden
                var links = linkList.Where(x => 
                        x.ChangeNumber < item.ChangeNumber &&
                        (x.SourceItemId == item.Id || x.TargetItemId== item.Id)
                    ).ToList();
                if (links.Count != 0)
                {
                    result.Add(item);
                }
            }
            return result;
        }

        private List<AbstractItem> GetDeletes()
        {
            var result = new List<AbstractItem>();
            var itemTableModel = AbstractItemTableModel.GetDefault();
            foreach (var item in itemTableModel.GetAllItems())
            {
                var i = allItems.Where(x => x.EntityId == item.EntityId);
                if (i == null)
                    result.Add(item);
            }
            return result;
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

        private void AfterDelete(AbstractItem sourceItem, AbstractItem targetItem)
        {
            AbstractItemTableModel.GetDefault().DeleteItem(sourceItem.Id);
            AbstractItemTableModel.GetDefault().DeleteItem(targetItem.Id);
            var link = linkList.Where(x => x.SourceItemId == sourceItem.Id || x.TargetItemId == targetItem.Id).First();
            LinkStatusTableModel.GetDefault().DeleteItem(link.Id);
        }
    }
}