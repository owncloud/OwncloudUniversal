using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLitePCL;
using OwncloudUniversal.Model;
using OwncloudUniversal.Synchronization.SQLite;

namespace OwncloudUniversal.Synchronization.Model
{
    public class FolderAssociationTableModel : AbstractTableModelBase<FolderAssociation, long>
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
            query.Bind(1, item.LocalFolderId);
            query.Bind(2, item.RemoteFolderId);
            query.Bind(3, item.IsActive ? 1 : 0);
            query.Bind(4, (long)item.SyncDirection);
            query.Bind(5, DateTimeHelper.DateTimeSQLite(item.LastSync));
            query.Bind(6, item.SupportsInstantUpload ? 1 : 0);
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
            query.Bind(1, item.LocalFolderId);
            query.Bind(2, item.RemoteFolderId);
            query.Bind(3, item.IsActive ? 1 : 0);
            query.Bind(4, (long)item.SyncDirection);
            query.Bind(5, SQLite.DateTimeHelper.DateTimeSQLite(item.LastSync));
            query.Bind(6, item.SupportsInstantUpload ? 1 : 0);
            query.Bind(7, item.Id);
        }

        protected override FolderAssociation CreateInstance(ISQLiteStatement query)
        {
            FolderAssociation fa = new FolderAssociation();
            fa.Id = (long)query[0];
            fa.LocalFolderId = (long)query[1];
            fa.RemoteFolderId = (long)query[2];
            fa.IsActive = (long)query[3] == 1;
            fa.SyncDirection = (SyncDirection)Enum.Parse(typeof(SyncDirection), (string)query[4]);
            DateTime date;
            if(!DateTime.TryParse((string)query[5], out date))
                date = DateTime.MinValue;
            fa.LastSync = date;
            fa.SupportsInstantUpload = (long) query[6] == 1;
            return fa;
        }

        protected override string GetDeleteItemQuery()
        {
            return "DELETE FROM Association WHERE Id = ?";
        }

        protected override string GetInsertItemQuery()
        {
            return "INSERT INTO Association (LocalItemId, RemoteItemId, IsActive, SyncDirection, LastSync, SupportsInstantUpload) VALUES(@localitemid, @remoteitemid, @isactive, @syncdirection, @lastsync, @SupportsInstantUpload)";
        }

        protected override string GetSelectAllQuery()
        {
            return "SELECT Id, LocalItemId, RemoteItemId, IsActive, SyncDirection, LastSync, SupportsInstantUpload FROM Association";
        }

        protected override string GetSelectItemQuery()
        {
            return "SELECT Id, LocalItemId, RemoteItemId, IsActive, SyncDirection, LastSync, SupportsInstantUpload FROM Association WHERE Id = ?";
        }

        protected override string GetUpdateItemQuery()
        {
            return "UPDATE Association  SET LocalItemId = ?, RemoteItemId = ?, IsActive = ?, SyncDirection = ?, LastSync= ?, SupportsInstantUpload = ? WHERE Id = ?";
        }

        protected override string GetLastInsertRowIdQuery()
        {
            return "SELECT last_insert_rowid() FROM Association";
        }

        protected override string GetSelectByEntityIdQuery()
        {
            throw new NotSupportedException();
        }

        protected override void BindSelectByPathQuery(ISQLiteStatement query, string path, long folderId)
        {
            throw new NotSupportedException();
        }

        protected override void BindSelectItemQuery(ISQLiteStatement query, string itemId)
        {
            throw new NotImplementedException();
        }
    }
}
