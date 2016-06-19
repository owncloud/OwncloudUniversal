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
            query.Bind(1, item.LocalPath);
            query.Bind(2, item.RemotePath);
            query.Bind(3, item.IsActive ? 1 : 0);
            query.Bind(4, (int)item.SyncDirection);
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
            query.Bind(1, item.LocalPath);
            query.Bind(2, item.RemotePath);
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

            fa.LocalPath = (string)query[1];
            fa.RemotePath = (string)query[2];
            return fa;
        }

        protected override string GetDeleteItemQuery()
        {
            return "DELETE FROM Association WHERE Id = ?";
        }

        protected override string GetInsertItemQuery()
        {
            return "INSERT INTO Association (LocalPath, RemotePath, IsActive, SyncDirection) VALUES(@localitemid, @remoteitemid, @isactive, @syncdirection)";
        }

        protected override string GetSelectAllQuery()
        {
            return "SELECT Id, LocalPath, RemotePath, IsActive, SyncDirection FROM Association";
        }

        protected override string GetSelectItemQuery()
        {
            return "SELECT Id, LocalPath, RemotePath, IsActive, SyncDirection FROM Association WHERE Id = ?";
        }

        protected override string GetUpdateItemQuery()
        {
            return "UPDATE Association  SET LocalPath = ?, RemotePath = ?, IsActive = ?, SyncDirection = ? WHERE Id = ?";
        }

        protected override string GetLastInsertRowIdQuery()
        {
            return "SELECT last_insert_rowid() FROM Association";
        }
    }
}
