using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using SQLitePCL;
using OwncloudUniversal.Shared.SQLite;

namespace OwncloudUniversal.Shared.Model
{
    public abstract class AbstractTableModelBase<TItem, TKey>
    {
        protected abstract TItem CreateInstance(ISQLiteStatement query);

        protected abstract string GetSelectItemQuery();
        protected abstract void BindSelectItemQuery(ISQLiteStatement query, TKey key);
        protected abstract void BindSelectItemQuery(ISQLiteStatement query, string itemId);

        protected abstract string GetInsertItemQuery();
        protected abstract void BindInsertItemQuery(ISQLiteStatement query, TItem item);

        protected abstract string GetUpdateItemQuery();
        protected abstract void BindUpdateItemQuery(ISQLiteStatement query, TItem item, TKey key);

        protected abstract string GetDeleteItemQuery();
        protected abstract void BindDeleteItemQuery(ISQLiteStatement query, TKey key);

        protected abstract string GetSelectAllQuery();
        protected abstract void BindSelectAllQuery(ISQLiteStatement query);

        protected ISQLiteConnection Connection { get { return SQLiteClient.Connection; } }

        public TItem GetItem(TKey key)
        {
            using(var query = Connection.Prepare(GetSelectItemQuery()))
            {
                BindSelectItemQuery(query, key);
                if(query.Step() == SQLiteResult.ROW)
                {
                    var item = CreateInstance(query);
                    return item;
                }
            }
            return default(TItem);
        }

        public void InsertItem(TItem item)
        {
            using (var query = Connection.Prepare(GetInsertItemQuery()))
            {
                BindInsertItemQuery(query, item);
                query.Step();
            }
        }

        public void UpdateItem(TItem item, TKey key)
        {
            using(var query = Connection.Prepare(GetUpdateItemQuery()))
            {
                BindUpdateItemQuery(query, item, key);
                query.Step();
            }
        }

        public void DeleteItem(TKey key)
        {
            using(var query = Connection.Prepare(GetDeleteItemQuery()))
            {
                BindDeleteItemQuery(query, key);
                query.Step();
            }
        }

        public TItem GetLastInsertItem()
        {
            using (var query = Connection.Prepare(GetLastInsertRowIdQuery()))
            {
                if (query.Step() == SQLiteResult.ROW)
                {
                    var item = GetItem((TKey)query[0]);
                    return item;
                }

            }
            throw new ArgumentOutOfRangeException("Key not found");
        }

        public ObservableCollection<TItem> GetAllItems()
        {
            var items = new ObservableCollection<TItem>();
            using(var query = Connection.Prepare(GetSelectAllQuery()))
            {
                BindSelectAllQuery(query);
                while(query.Step() == SQLiteResult.ROW)
                {
                    var item = CreateInstance(query);
                    items.Add(item);
                }
            }
            return items;
        }

        protected abstract string GetLastInsertRowIdQuery();
        protected abstract string GetSelectByEntityIdQuery();
        protected abstract void BindSelectByPathQuery(ISQLiteStatement query, string path, TKey folderId);


    }
}
