using owncloud_universal.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLitePCL;
using System.Collections.ObjectModel;

namespace owncloud_universal.Model
{
    class LocalItemTableModel : AbstractTableModelBase<LocalItem, long>
    {
        private LocalItemTableModel() { }

        private static LocalItemTableModel instance;

        public static LocalItemTableModel GetDefault()
        {
            lock (typeof(LocalItemTableModel))
            {
                if (instance == null)
                    instance = new LocalItemTableModel();
                return instance;
            }
        }

        protected override void BindDeleteItemQuery(ISQLiteStatement query, long key)
        {
            query.Bind(1, key);
        }

        protected override void BindInsertItemQuery(ISQLiteStatement query, LocalItem item)
        {
            query.Bind(1, SQLite.DateTimeHelper.DateTimeSQLite(item.LastModified));
            query.Bind(2, (item.IsCollection ? 1 : 0));
            query.Bind(3, item.Path);
            query.Bind(4, item.FolderId);
            query.Bind(5, item.RemoteItemId);
        }

        protected override void BindSelectAllQuery(ISQLiteStatement query)
        {
            //nothing
        }

        protected override void BindSelectItemQuery(ISQLiteStatement query, long key)
        {
            query.Bind(1, key);
        }

        protected override void BindUpdateItemQuery(ISQLiteStatement query, LocalItem item, long key)
        {
            query.Bind(1, SQLite.DateTimeHelper.DateTimeSQLite(item.LastModified));
            query.Bind(2, (item.IsCollection ? 1 : 0));
            query.Bind(3, item.Path);
            query.Bind(4, item.FolderId);
            query.Bind(5, item.RemoteItemId);
            query.Bind(6, key);
        }

        protected override void BindSelectByPathQuery(ISQLiteStatement query, string path, long folderId)
        {
            query.Bind(1, path);
            query.Bind(2, folderId);
        }
        protected override LocalItem CreateInstance(ISQLiteStatement query)
        {
            LocalItem i = new LocalItem
            {
                Id = (long) query[0],
                LastModified = Convert.ToDateTime(query[1]),
                IsCollection = (long)query[2] == 1,
                Path = (string)query[3],
                FolderId = (long) query[4],
                RemoteItemId = (long) query[5]
            };
            return i;
        }

        protected override string GetDeleteItemQuery()
        {
            return "DELETE FROM LocalItem WHERE Id = ?";
        }

        protected override string GetInsertItemQuery()
        {
            return "INSERT INTO LocalItem (LastModified, IsCollection, Path, FolderId, RemoteItemId) VALUES (@lastmodified, @iscollection, @path, @folderid, @remoteitemid)";
        }

        protected override string GetSelectAllQuery()
        {
            return "SELECT Id, LastModified, IsCollection, Path, FolderId, RemoteItemId FROM LocalItem";
        }

        protected override string GetSelectItemQuery()
        {
            return "SELECT Id, LastModified, IsCollection, Path, FolderId, RemoteItemId FROM LocalItem WHERE Id = ?";
        }

        protected override string GetUpdateItemQuery()
        {
            return "UPDATE LocalItem SET LastModified = ?, IsCollection = ?, Path = ?, FolderId = ?, RemoteItemId = ? WHERE Id= ?";
        }

        protected override string GetLastInsertRowIdQuery()
        {
            return "SELECT last_insert_rowid() FROM LocalItem";
        }

        protected override string GetSelectByPathQuery()
        {
            return "SELECT  Id, LastModified, IsCollection, Path, FolderId, RemoteItemId FROM LocalItem WHERE Path = ? AND FolderId = ?";
        }

        protected override string GetGetInsertsQuery()
        {
            return "SELECT Id, LastModified, IsCollection, Path, FolderId, RemoteItemId FROM LocalItem WHERE RemoteItemId = 0 AND FolderId = ?";
        }

        protected override void BindGetInsertsQuery(ISQLiteStatement query, long folderId)
        { 
            query.Bind(1, folderId);
        }

        protected override string GetGetUpdatesQuery()
        {
            return "SELECT Id, LastModified, IsCollection, Path, FolderId, RemoteItemId FROM LocalItem WHERE LastModified > ? AND FolderId = ?";
        }

        protected override void BindGetUpdatesQuery(ISQLiteStatement query, object date, long folderId)
        {
            query.Bind(1, (DateTime)date);
            query.Bind(1, folderId);
        }

        protected override string GetGetDeletesQuery()
        {
            throw new NotImplementedException();
        }

        protected override void BindGetDeletesQuery()
        {
            throw new NotImplementedException();
        }
    }
}
