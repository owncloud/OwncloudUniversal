using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLitePCL;

namespace owncloud_universal.Model
{
    class SyncItemTableModel : AbstractTableModelBase<SyncItem, long>
    {
        private SyncItemTableModel() { }

        private static SyncItemTableModel instance;

        public static SyncItemTableModel GetDefault()
        {
            lock (typeof(SyncItemTableModel))
            {
                if (instance == null)
                    instance = new SyncItemTableModel();
                return instance;
            }
        }

        protected override void BindDeleteItemQuery(ISQLiteStatement query, long key)
        {
            query.Bind(1, key);
        }

        protected override void BindInsertItemQuery(ISQLiteStatement query, SyncItem item)
        {
            query.Bind(1, SQLite.DateTimeHelper.DateTimeSQLite(item.LastModified));
            query.Bind(2, (item.IsCollection ? 1 : 0));
            query.Bind(3, item.Path);
            query.Bind(4, item.RelativePath);
            query.Bind(5, item.Etag);
            query.Bind(4, item.FolderId);
        }

        protected override void BindSelectAllQuery(ISQLiteStatement query)
        {
            //throw new NotImplementedException();
        }

        protected override void BindSelectByPathQuery(ISQLiteStatement query, string path, long folderId)
        {
            query.Bind(1, path);
            query.Bind(2, folderId);
        }

        protected override void BindSelectItemQuery(ISQLiteStatement query, long key)
        {
            query.Bind(1, key);
        }

        protected override void BindUpdateItemQuery(ISQLiteStatement query, SyncItem item, long key)
        {
            query.Bind(1, SQLite.DateTimeHelper.DateTimeSQLite(item.LastModified));
            query.Bind(2, (item.IsCollection ? 1 : 0));
            query.Bind(3, item.Path);
            query.Bind(4, item.RelativePath);
            query.Bind(5, item.Etag);
            query.Bind(6, item.FolderId);
            query.Bind(7, key);
        }

        protected override SyncItem CreateInstance(ISQLiteStatement query)
        {
            SyncItem i = new SyncItem
            {
                Id = (long)query[0],
                LastModified = Convert.ToDateTime(query[1]),
                IsCollection = (long)query[2] == 1,
                Path = (string)query[3],
                RelativePath = (string)query[4],
                Etag = (string)query[5],
                FolderId = (long)query[6]
            };
            return i;
        }

        protected override string GetDeleteItemQuery()
        {
            return "DELETE FROM SyncItem WHERE Id = ?";
        }

        protected override string GetInsertItemQuery()
        {
            return "INSTER INTO SyncItem (LastModified, IsCollection, Path, RelativePath, Etag, FolderId) VALUES(@lastmodified, @iscollection, @path, @relativepath, @etag, @folderid)";
        }

        protected override string GetLastInsertRowIdQuery()
        {
            return "SELECT last_insert_rowid() FROM SyncItem";
        }

        protected override string GetSelectAllQuery()
        {
            return "SELECT Id, LastModified, IsCollection, Path, RelativePath, Etag, FolderId FROM SyncItem";
        }

        protected override string GetSelectByPathQuery()
        {
            return "SELECT Id, LastModified, IsCollection, Path, RelativePath, Etag, FolderId FROM SyncItem WHERE RelativePath = ?";
        }

        protected override string GetSelectItemQuery()
        {
            return "SELECT Id, LastModified, IsCollection, Path, RelativPath, Etag, FolderId FROM SyncItem WHERE Id = ?";
        }

        protected override string GetUpdateItemQuery()
        {
            return "UPDATE SyncItem SET LastModified = ?, IsCollection = ?, Path = ?, RelativePath = ?, Etag = ?, FolderId = ? WHERE Id = ?";
        }
    }
}
