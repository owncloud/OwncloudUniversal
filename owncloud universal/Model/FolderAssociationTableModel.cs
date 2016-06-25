using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLitePCL;
using owncloud_universal.Model;

namespace owncloud_universal.Model
{
    class FolderAssociationTableModel : AbstractTableModelBase<FolderAssociation, long>
    {
        private FolderAssociationTableModel() { }

        private static FolderAssociationTableModel instance;

        public static FolderAssociationTableModel GetDefault()
        {
            lock (typeof(FolderAssociationTableModel))
            {
                if (instance == null)
                    instance = new FolderAssociationTableModel();
                return instance;
            }
        }

        protected override void BindDeleteItemQuery(ISQLiteStatement query, long key)
        {
            query.Bind(1, key);
        }

        protected override void BindInsertItemQuery(ISQLiteStatement query, FolderAssociation item)
        {
            query.Bind(1, item.Id);
            query.Bind(2, item.LocalFolder.Id);
            query.Bind(3, item.RemoteFolder.Id);
            query.Bind(4, item.IsActive ? 1 : 0);
            query.Bind(5, (int)item.SyncDirection);
        }

        protected override void BindSelectAllQuery(ISQLiteStatement query)
        {
            //nix
        }

        protected override void BindSelectItemQuery(ISQLiteStatement query, long key)
        {
            query.Bind(1, key);
        }

        protected override void BindUpdateItemQuery(ISQLiteStatement query, FolderAssociation item, long key)
        {
            query.Bind(1, item.LocalFolder.Id);
            query.Bind(2, item.RemoteFolder.Id);
            query.Bind(3, item.IsActive ? 1 : 0);
            query.Bind(4, (int)item.SyncDirection);
            query.Bind(5, item.Id);
        }

        protected override FolderAssociation CreateInstance(ISQLiteStatement query)
        {
            FolderAssociation fa = new FolderAssociation();
            fa.Id = (long)query[0];
            fa.IsActive = (long)query[3] == 1;
            fa.SyncDirection = (SyncDirection)Enum.Parse(typeof(SyncDirection), (string)query[4]);


            var li = LocalItemTableModel.GetDefault();
            fa.LocalFolder = li.GetItem((long)query[1]);

            var ri = RemoteItemTableModel.GetDefault();
            fa.RemoteFolder = ri.GetItem((long)query[2]);

            return fa;
        }

        protected override string GetDeleteItemQuery()
        {
            return "DELETE FROM Association WHERE Id = ?";
        }

        protected override string GetInsertItemQuery()
        {
            return "INSERT INTO Association (Id, LocalItemId, RemoteItemId, IsActive, SyncDirection) VALUES(@id, @localitemid, @remoteitemid, @isactive, @syncdirection)";
        }

        protected override string GetSelectAllQuery()
        {
            return "SELECT Id, LocalItemId, RemoteItemId, IsActive, SyncDirection FROM Association";
        }

        protected override string GetSelectItemQuery()
        {
            return "SELECT Id, LocalItemId, RemoteItemId, IsActive, SyncDirection FROM Association WHERE Id = ?";
        }

        protected override string GetUpdateItemQuery()
        {
            return "UPDATE Association  SET LocalItemId = ?, RemoteItemId = ?, IsActive = ?, SyncDirection = ? WHERE Id = ?";
        }

        protected override string GetLastInsertRowIdQuery()
        {
            return "SELECT last_insert_rowid() FROM Association";
        }
    }
}
