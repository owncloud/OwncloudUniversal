using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLitePCL;
using System.Collections.ObjectModel;

namespace owncloud_universal.Model
{
    class LinkStatusTableModel : AbstractTableModelBase<LinkStatus, long>
    {
        private LinkStatusTableModel() { }

        private static LinkStatusTableModel instance;
        protected override void BindDeleteItemQuery(ISQLiteStatement query, long key)
        {
            query.Bind(1, key);
        }

        protected override void BindInsertItemQuery(ISQLiteStatement query, LinkStatus item)
        {
            //"INSERT INTO LinkStatus (TargetItemId, SourceItemId, ChangeNumber, AssociationId) VALUES(@targetitemid, @sourceitemid, @changenumber, @associationid)";
            query.Bind(1, item.TargetItemId);
            query.Bind(2, item.SourceItemId);
            query.Bind(3, item.ChangeNumber);
            query.Bind(4, item.AssociationId);
        }

        protected override void BindSelectAllQuery(ISQLiteStatement query)
        {
            //nix
        }

        protected override void BindSelectByPathQuery(ISQLiteStatement query, string path, long folderId)
        {
            throw new NotImplementedException();
        }

        protected override void BindSelectItemQuery(ISQLiteStatement query, string itemId)
        {
            query.Bind(1, itemId);
        }

        protected override void BindSelectItemQuery(ISQLiteStatement query, long key)
        {
            query.Bind(1, key);
        }

        protected override void BindUpdateItemQuery(ISQLiteStatement query, LinkStatus item, long key)
        {
            //UPDATE LinkStatus SET TargetItemId = ?, SourceItemId = ?, ChangeNumber = ?, AssociationId = ? WHERE Id = ?
            query.Bind(1, item.TargetItemId);
            query.Bind(2, item.SourceItemId);
            query.Bind(3, item.ChangeNumber);
            query.Bind(4, item.AssociationId);
            query.Bind(5, key);
        }

        protected override LinkStatus CreateInstance(ISQLiteStatement query)
        {
            var link = new LinkStatus();
            link.Id = (long)query["Id"];
            link.TargetItemId = (long)query["TargetItemId"];
            link.SourceItemId = (long)query["SourceItemId"];
            link.ChangeNumber = (long)query["ChangeNumber"];
            link.AssociationId = (long)query["AssociationId"];
            return link;
        }

        protected override string GetDeleteItemQuery()
        {
            return "DELETE FROM LinkStatus WHERE Id = ?";
        }

        protected override string GetInsertItemQuery()
        {
            return "INSERT INTO LinkStatus (TargetItemId, SourceItemId, ChangeNumber, AssociationId) VALUES(@targetitemid, @sourceitemid, @changenumber, @associationid)";
        }

        protected override string GetLastInsertRowIdQuery()
        {
            return "SELECT last_insert_rowid() FROM LinkStatus";
        }

        public static LinkStatusTableModel GetDefault()
        {
            //lock (typeof(LinkStatusTableModel))
            //{
                if (instance == null)
                    instance = new LinkStatusTableModel();
                return instance;
            //}
        }

        protected override string GetSelectAllQuery()
        {
            return "SELECT Id, TargetItemId, SourceItemId, ChangeNumber, AssociationId FROM LinkStatus";
        }

        protected override string GetSelectByPathQuery()
        {
            throw new NotImplementedException();
        }

        protected override string GetSelectItemQuery()
        {
            return "SELECT Id, TargetItemId, SourceItemId, ChangeNumber, AssociationId FROM LinkStatus WHERE Id = ?";
        }

        protected override string GetUpdateItemQuery()
        {
            return "UPDATE LinkStatus SET TargetItemId = ?, SourceItemId = ?, ChangeNumber = ?, AssociationId = ? WHERE Id = ?";
        }

        public LinkStatus GetItem(AbstractItem sourceItem)
        {
            var statement = "SELECT Id, TargetItemId, SourceItemId, ChangeNumber, AssociationId FROM LinkStatus WHERE SourceItemId = ?";
            using (var query = Connection.Prepare(statement))
            {
                BindSelectItemQuery(query, sourceItem.EntityId);
                if (query.Step() == SQLiteResult.ROW)
                {
                    var i = CreateInstance(query);
                    return i;
                }
            }
            throw new ArgumentOutOfRangeException("Key not found");
        }

        public ObservableCollection<LinkStatus> GetAllItems(FolderAssociation association)
        {
            var items = new ObservableCollection<LinkStatus>();
            using (var query = Connection.Prepare(GetSelectAllQuery() + " WHERE AssociationId = ?"))
            {
                BindSelectAllQuery(query);
                query.Bind(6, association.Id);
                while (query.Step() == SQLiteResult.ROW)
                {
                    var item = CreateInstance(query);
                    items.Add(item);
                }
            }
            return items;
        }
    }
}
