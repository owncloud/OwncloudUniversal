using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLitePCL;

namespace owncloud_universal.Model
{
    class AbstractItemTableModel : AbstractTableModelBase<AbstractItem, int>
    {
        private AbstractItemTableModel() { }

        private static AbstractItemTableModel instance;
        public static AbstractItemTableModel GetDefault()
        {
            lock (typeof(AbstractItemTableModel))
            {
                if (instance == null)
                    instance = new AbstractItemTableModel();
                return instance;
            }
        }
        protected override void BindDeleteItemQuery(ISQLiteStatement query, int key)
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

        protected override void BindSelectItemQuery(ISQLiteStatement query, int key)
        {
            throw new NotImplementedException();
        }

        protected override string GetDeleteItemQuery()
        {
            return "DELETE from Item where Id = ?";
        }
             
        protected override string GetInsertItemQuery()
        {
            return "INSERT INTO Item (FolderId, ItemId, IsCollection, ChangeKey, ItemId) VALUES(@folderid, @itemid, @iscollection, @changekey, @itemid)";
        }

        protected override string GetLastInsertRowIdQuery()
        {
            throw new NotImplementedException();
        }

        protected override string GetSelectAllQuery()
        {
            return "";
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

        protected override void BindSelectItemQuery(ISQLiteStatement query, string itemId)
        {
            throw new NotImplementedException();
        }

        protected override void BindInsertItemQuery(ISQLiteStatement query, AbstractItem item)
        {
            throw new NotImplementedException();
        }

        protected override void BindUpdateItemQuery(ISQLiteStatement query, AbstractItem item, int key)
        {
            throw new NotImplementedException();
        }

        protected override AbstractItem CreateInstance(ISQLiteStatement query)
        {
            throw new NotImplementedException();
        }
    }
}
