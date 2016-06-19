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
        }

        protected override RemoteItem CreateInstance(ISQLiteStatement query)
        {
            WebDav.DavItem di = new WebDav.DavItem
            {
                Etag = (string)query[1],
                IsCollection = (long)query[2] == 1,
                Href = (string)query[3],
                DisplayName = (string)query[4],
                LastModified = Convert.ToDateTime(query[5])
            };
            RemoteItem item = new RemoteItem(di) {Id = (long) query[0]};
            return item;
        }

        protected override string GetDeleteItemQuery()
        {
            return "DELETE FROM RemoteItem WHERE Id = ?";
        }

        protected override string GetInsertItemQuery()
        {
            return "INSERT INTO RemoteItem (Etag, IsCollection, Href, DisplayName, LastModified) VALUES (@etag, @iscollection, @href, @displayname, @lastmodified)";
        }

        protected override string GetSelectAllQuery()
        {
            return "SELECT Id, Etag, IsCollection, Href, DisplayName, LastModified FROM RemoteItem";
        }

        protected override string GetSelectItemQuery()
        {
            return "SELECT Id, Etag, IsCollection, Href, DisplayName, LastModified FROM RemoteItem WHERE Id = ?";
        }
        
        protected override string GetUpdateItemQuery()
        {
            return "UPDATE RemoteItem SET Etag = ?, IsCollection = ?, Href = ?, DisplayName = ?, LastModified = ?";
        }

        protected override string GetLastInsertRowIdQuery()
        {
            return "SELECT last_insert_rowid() from RemoteItem";
        }
    }
}
