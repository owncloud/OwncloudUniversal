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
            foreach (var item in items)
            {
                allItems = await _webDavAdapter.GetAllItems(item);
                allItems.AddRange(await _fileSystemAdapter.GetAllItems(item));
                _UpdateFileIndexes(item);
                var model = LinkStatusTableModel.GetDefault();
                linkList = model.GetAllItems(item).ToList();

                var inserts = GetInserts();
                var updates = GetUpdates();
                var deltes = GetDeletes();
            }
        }  
        
        private void DoInserts(List<AbstractItem> items)
        {
            foreach (var item in items)
            {
                if(item.GetType() == typeof(LocalItem))
                {
                    _webDavAdapter.AddItem(item);
                }
                else
                {
                    _fileSystemAdapter.AddItem(item);
                }
            }
        }

        private void DoUpdates(List<AbstractItem> items)
        {
            foreach (var item in items)
            {
                if (item.GetType() == typeof(LocalItem))
                {
                    _webDavAdapter.UpdateItem(item);
                }
                else
                {
                    _fileSystemAdapter.UpdateItem(item);
                }
            }
        }

        private void DoDeletes(List<AbstractItem> items)
        {
            foreach (var item in items)
            {
                if (item.GetType() == typeof(LocalItem))
                {
                    _webDavAdapter.DeleteItem(item);
                }
                else
                {
                    _fileSystemAdapter.DeleteItem(item);
                }
            }
        }

        private void _UpdateFileIndexes(FolderAssociation association)
        {

            var itemTableModel = AbstractItemTableModel.GetDefault();
            
            foreach (var item in allItems)
            {
                var foundItem = itemTableModel.GetItem(item.EntityId);
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
                var links = linkList.Where(x => x.SourceItemId == item.EntityId || x.TargetItemId == item.EntityId).ToList();
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
                        (x.SourceItemId == item.EntityId || x.TargetItemId== item.Id)
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
    }
}