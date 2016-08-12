using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLitePCL;
using System.Collections.ObjectModel;

namespace owncloud_universal.Model
{
    class LinkStatusTableModel : AbstractTableModelBase<LinkStatus, int>
    {
        private LinkStatusTableModel() { }

        private static LinkStatusTableModel instance;
        protected override void BindDeleteItemQuery(ISQLiteStatement query, int key)
        {
            throw new NotImplementedException();
        }

        protected override void BindInsertItemQuery(ISQLiteStatement query, LinkStatus item)
        {
            throw new NotImplementedException();
        }

        protected override void BindSelectAllQuery(ISQLiteStatement query)
        {
            throw new NotImplementedException();
        }

        protected override void BindSelectByPathQuery(ISQLiteStatement query, string path, int folderId)
        {
            throw new NotImplementedException();
        }

        protected override void BindSelectItemQuery(ISQLiteStatement query, string itemId)
        {
            throw new NotImplementedException();
        }

        protected override void BindSelectItemQuery(ISQLiteStatement query, int key)
        {
            throw new NotImplementedException();
        }

        protected override void BindUpdateItemQuery(ISQLiteStatement query, LinkStatus item, int key)
        {
            throw new NotImplementedException();
        }

        protected override LinkStatus CreateInstance(ISQLiteStatement query)
        {
            throw new NotImplementedException();
        }

        protected override string GetDeleteItemQuery()
        {
            throw new NotImplementedException();
        }

        protected override string GetInsertItemQuery()
        {
            throw new NotImplementedException();
        }

        protected override string GetLastInsertRowIdQuery()
        {
            throw new NotImplementedException();
        }

        public static LinkStatusTableModel GetDefault()
        {
            lock (typeof(LinkStatusTableModel))
            {
                if (instance == null)
                    instance = new LinkStatusTableModel();
                return instance;
            }
        }

        protected override string GetSelectAllQuery()
        {
            throw new NotImplementedException();
        }

        protected override string GetSelectByPathQuery()
        {
            throw new NotImplementedException();
        }

        protected override string GetSelectItemQuery()
        {
            throw new NotImplementedException();
        }

        protected override string GetUpdateItemQuery()
        {
            throw new NotImplementedException();
        }

        public new LinkStatus GetItem(string key)
        {
            return null;
        }

        public ObservableCollection<LinkStatus> GetAllItems(FolderAssociation association)
        {
            throw new NotImplementedException();
        }
    }
}
