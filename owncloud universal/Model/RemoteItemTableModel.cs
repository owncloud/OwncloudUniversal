using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLitePCL;

namespace owncloud_universal.Model
{
    class RemoteItemTableModel : AbstractTableModelBase<RemoteItem, long>
    {
        private RemoteItemTableModel() { }

        private static RemoteItemTableModel instance;

        public static RemoteItemTableModel GetDefault()
        {
            lock (typeof(RemoteItemTableModel))
            {
                if (instance == null)
                    instance = new RemoteItemTableModel();
                return instance;
            }
        }

        protected override void BindDeleteItemQuery(ISQLiteStatement query, long key)
        {
            query.Bind(1, key);
        }

        protected override void BindInsertItemQuery(ISQLiteStatement query, RemoteItem item)
        {
            query.Bind(1, item.DavItem.Etag);
            query.Bind(2, item.DavItem.IsCollection ? 1 : 0);
            query.Bind(3, item.DavItem.Href);
            query.Bind(4, item.DavItem.DisplayName);
            query.Bind(5, SQLite.DateTimeHelper.DateTimeSQLite(item.DavItem.LastModified));
            query.Bind(6, item.FolderId);
            query.Bind(7, item.LocalItemId);
        }

        protected override void BindSelectAllQuery(ISQLiteStatement query)
        {
            //nix
        }

        protected override void BindSelectItemQuery(ISQLiteStatement query, long key)
        {
            query.Bind(1, key);
        }

        protected override void BindUpdateItemQuery(ISQLiteStatement query, RemoteItem item, long key)
        {
            query.Bind(1, item.DavItem.Etag);
            query.Bind(2, item.DavItem.IsCollection ? 1 : 0);
            query.Bind(3, item.DavItem.Href);
            query.Bind(4, item.DavItem.DisplayName);
            query.Bind(5, SQLite.DateTimeHelper.DateTimeSQLite(item.DavItem.LastModified));
            query.Bind(6, item.FolderId);
            query.Bind(7, item.LocalItemId);
            query.Bind(8, key);
        }

        protected override RemoteItem CreateInstance(ISQLiteStatement query)
        {
            WebDav.DavItem di = new WebDav.DavItem
            {
                Etag = (string)query["Etag"],
                IsCollection = (long)query["IsCollection"] == 1,
                Href = (string)query["Href"],
                DisplayName = (string)query["DisplayName"]
            };
            var date = query["LastModified"] as DateTime?;
            if (date != null)
                di.LastModified = Convert.ToDateTime(date);
            RemoteItem item = new RemoteItem(di)
            {
                Id = (long)query["Id"],
                FolderId = (long)query["FolderId"],
                LocalItemId = (long)query["LocalItemId"]
            };
            return item;
        }

        protected override string GetDeleteItemQuery()
        {
            return "DELETE FROM RemoteItem WHERE Id = ?";
        }

        protected override string GetInsertItemQuery()
        {
            return "INSERT INTO RemoteItem (Etag, IsCollection, Href, DisplayName, LastModified, FolderId, LocalItemId) VALUES (@etag, @iscollection, @href, @displayname, @lastmodified, @folderid, @localitemid)";
        }

        protected override string GetSelectAllQuery()
        {
            return "SELECT Id, Etag, IsCollection, Href, DisplayName, LastModified, FolderId, LocalItemId FROM RemoteItem";
        }

        protected override string GetSelectItemQuery()
        {
            return "SELECT Id, Etag, IsCollection, Href, DisplayName, LastModified, FolderId, LocalItemId FROM RemoteItem WHERE Id = ?";
        }

        protected override string GetUpdateItemQuery()
        {
            return "UPDATE RemoteItem SET Etag = ?, IsCollection = ?, Href = ?, DisplayName = ?, LastModified = ?, FolderId = ?, LocalItemId = ? WHERE Id = ?";
        }

        protected override string GetLastInsertRowIdQuery()
        {
            return "SELECT last_insert_rowid() from RemoteItem";
        }

        protected override string GetSelectByPathQuery()
        {
            return "SELECT Id, Etag, IsCollection, Href, DisplayName, LastModified, FolderId, LocalItemId FROM RemoteItem WHERE Href = ? AND FolderId = ?";
        }

        protected override void BindSelectByPathQuery(ISQLiteStatement query, string path, long folderId)
        {
            query.Bind(1, path);
            query.Bind(2, folderId);
        }

        protected override string GetGetInsertsQuery()
        {
            return "SELECT Id, Etag, IsCollection, Href, DisplayName, LastModified, FolderId, LocalItemId FROM RemoteItem WHERE LocalItemId = 0 AND FolderId = ?";
        }

        protected override void BindGetInsertsQuery(ISQLiteStatement query, long folderId)
        {
            query.Bind(1, folderId);
        }

        protected override string GetGetUpdatesQuery()
        {
            throw new NotImplementedException();
        }

        protected override void BindGetUpdatesQuery(ISQLiteStatement query, object value, long folderId)
        {
            throw new NotImplementedException();
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
