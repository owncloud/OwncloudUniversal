using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLitePCL;

namespace OwncloudUniversal.Shared.Model
{
    public class AbstractItemTableModel : AbstractTableModelBase<AbstractItem, long>
    {
        private AbstractItemTableModel() { }

        private static AbstractItemTableModel instance;
        public static AbstractItemTableModel GetDefault()
        {
            lock (typeof(AbstractItemTableModel))
            {
                if (instance == null)
                    instance = new AbstractItemTableModel();
                return new AbstractItemTableModel();
        }
    }
        protected override void BindDeleteItemQuery(ISQLiteStatement query, long key)
        {
            query.Bind(1, key);
        }

        protected override void BindSelectAllQuery(ISQLiteStatement query)
        {
            //nix
        }

        protected override void BindSelectByPathQuery(ISQLiteStatement query, string path, long folderId)
        {
            throw new NotImplementedException();
        }

        protected override void BindSelectItemQuery(ISQLiteStatement query, long key)
        {
            query.Bind(1, key);
        }

        protected override string GetDeleteItemQuery()
        {
            return "DELETE from Item where Id = ?";
        }
             
        protected override string GetInsertItemQuery()
        {
            return "INSERT INTO Item (AssociationId, EntityId, IsCollection, ChangeKey, ChangeNumber) VALUES(@AssociationId, @EntityId, @iscollection, @changekey, @changenumber)";
        }

        protected override string GetLastInsertRowIdQuery()
        {
            return "SELECT last_insert_rowid() FROM Item";
        }

        protected override string GetSelectAllQuery()
        {
            return "SELECT Id, AssociationId, EntityId, IsCollection, ChangeKey, ChangeNumber FROM Item";
        }

        protected override string GetSelectByPathQuery()
        {
            //throw new NotImplementedException();
            return null;
        }

        protected override string GetSelectItemQuery()
        {
            return "SELECT Id, AssociationId, EntityId, IsCollection, ChangeKey, ChangeNumber FROM Item WHERE Id = ?";
        }

        protected override string GetUpdateItemQuery()
        {
            return "UPDATE Item SET EntityId = ?, IsCollection = ?, ChangeKey = ?, ChangeNumber = ? WHERE Id = ?";
        }

        protected override void BindSelectItemQuery(ISQLiteStatement query, string itemId)
        {
            query.Bind(1, itemId);
        }

        protected override void BindInsertItemQuery(ISQLiteStatement query, AbstractItem item)
        {
            //"INSERT INTO Item (AssociationId, ItemId, IsCollection, ChangeKey, ChangeNumber) VALUES(@AssociationId, @itemid, @iscollection, @changekey, @changenumber)";
            query.Bind(1, item.Association.Id);
            query.Bind(2, item.EntityId);
            query.Bind(3, item.IsCollection ? 1 : 0);
            query.Bind(4, item.ChangeKey);
            query.Bind(5, item.ChangeNumber);
        }

        protected override void BindUpdateItemQuery(ISQLiteStatement query, AbstractItem item, long key)
        {
            //"UPDATE Item SET EntityId = ?, IsCollection = ?, ChangeKey = ?, ChangeNumber = ? WHERE Id = ?";
            query.Bind(1, item.EntityId);
            query.Bind(2, item.IsCollection ? 1 : 0);
            query.Bind(3, item.ChangeKey);
            query.Bind(4, item.ChangeNumber);
            query.Bind(5, key);
        }

        protected override AbstractItem CreateInstance(ISQLiteStatement query)
        {
            //SELECT Id, AssociationId, EntityId, IsCollection, ChangeKey, ChangeNumber FROM Item WHERE Id = ?
            var item = new AbstractItem();
            item.Id = (long)query["Id"];
            var associationModel = FolderAssociationTableModel.GetDefault();
            item.Association = associationModel.GetItem((long)query["AssociationId"]);
            item.EntityId = (string)(query["EntityId"] == null ? default(string) : query["EntityId"]);
            item.IsCollection = (long)query["IsCollection"] == 1;
            item.ChangeKey = (string)query["ChangeKey"];
            item.ChangeNumber = (long)query["ChangeNumber"];
            return item;
        }

        public AbstractItem GetItem(AbstractItem item)
        {
            using (var query = Connection.Prepare("SELECT Id, AssociationId, EntityId, IsCollection, ChangeKey, ChangeNumber FROM Item WHERE EntityId = ?"))
            {
                query.Bind(1, item.EntityId);
                if (query.Step() == SQLiteResult.ROW)
                {
                    item = CreateInstance(query);
                    return item;
                }
            }
            return null;
        }
    }
}
