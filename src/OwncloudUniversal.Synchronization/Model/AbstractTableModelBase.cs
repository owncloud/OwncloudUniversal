using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using SQLitePCL;
using OwncloudUniversal.Synchronization.SQLite;

namespace OwncloudUniversal.Synchronization.Model
{
    /// <summary>
    /// Represents a Table from the SQLite-Database
    /// </summary>
    /// <typeparam name="TItem">The type of the corresponding entity</typeparam>
    /// <typeparam name="TKey">The type of the primary key of the table</typeparam>
    public abstract class AbstractTableModelBase<TItem, TKey>
    {
        protected abstract TItem CreateInstance(ISQLiteStatement query);

        /// <summary>
        /// Gets the query string to select a specific item.
        /// </summary>
        /// <returns></returns>
        protected abstract string GetSelectItemQuery();

        /// <summary>
        /// Adds the parameters to the query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="key"></param>
        protected abstract void BindSelectItemQuery(ISQLiteStatement query, TKey key);

        /// <summary>
        /// Adds the parameters to the query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="itemId"></param>
        protected abstract void BindSelectItemQuery(ISQLiteStatement query, string itemId);

        /// <summary>
        /// Gets the query string to insert a new item
        /// </summary>
        /// <returns></returns>
        protected abstract string GetInsertItemQuery();

        /// <summary>
        /// Adds the parameters to the query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="item"></param>
        protected abstract void BindInsertItemQuery(ISQLiteStatement query, TItem item);

        /// <summary>
        /// Gets the query string to update an item
        /// </summary>
        /// <returns></returns>
        protected abstract string GetUpdateItemQuery();

        /// <summary>
        /// Adds the parameters to the query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="item"></param>
        /// <param name="key"></param>
        protected abstract void BindUpdateItemQuery(ISQLiteStatement query, TItem item, TKey key);
        
        /// <summary>
        /// Gets the query string to delete an item
        /// </summary>
        /// <returns></returns>
        protected abstract string GetDeleteItemQuery();

        /// <summary>
        /// Adds the parameters to the query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="key"></param>
        protected abstract void BindDeleteItemQuery(ISQLiteStatement query, TKey key);

        /// <summary>
        /// Gets the query string to select all items
        /// </summary>
        /// <returns></returns>
        protected abstract string GetSelectAllQuery();

        /// <summary>
        /// Adds the parameter to the query
        /// </summary>
        /// <param name="query"></param>
        protected abstract void BindSelectAllQuery(ISQLiteStatement query);

        /// <summary>
        /// Represents the connection to the SQLite-Database
        /// </summary>
        protected ISQLiteConnection Connection { get { return SQLiteClient.Connection; } }

        /// <summary>
        /// Gets an <see cref="TItem"/> from the table with a specified PrimaryKey
        /// </summary>
        /// <param name="key">The <see cref="TKey"/> of item to return</param>
        /// <returns>Return the item that has the specified <see cref="TKey"/></returns>
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
        /// <summary>
        /// Adds an new item to the table
        /// </summary>
        /// <param name="item">The item to add</param>
        public void InsertItem(TItem item)
        {
            using (var query = Connection.Prepare(GetInsertItemQuery()))
            {
                BindInsertItemQuery(query, item);
                query.Step();
            }
        }
        /// <summary>
        /// Updates an item in the table with a specified primary key
        /// </summary>
        /// <param name="item">The item containing the new values</param>
        /// <param name="key">The primary key of the item</param>
        public void UpdateItem(TItem item, TKey key)
        {
            using(var query = Connection.Prepare(GetUpdateItemQuery()))
            {
                BindUpdateItemQuery(query, item, key);
                query.Step();
            }
        }

        /// <summary>
        /// Deletes an item from the table
        /// </summary>
        /// <param name="key">The primary key of the item to delete</param>
        public void DeleteItem(TKey key)
        {
            using(var query = Connection.Prepare(GetDeleteItemQuery()))
            {
                BindDeleteItemQuery(query, key);
                query.Step();
            }
        }
        /// <summary>
        /// Return the last item that was inserted (used for tables with autoincrementing primary keys)
        /// </summary>
        /// <returns>The last item that was added to the table</returns>
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
        /// <summary>
        /// Returns all items from the table
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the query string to get the Id of the latest row
        /// </summary>
        /// <returns></returns>
        protected abstract string GetLastInsertRowIdQuery();

        /// <summary>
        /// Gets the query string to select an item with a specified EntityId
        /// </summary>
        /// <returns></returns>
        protected abstract string GetSelectByEntityIdQuery();

        /// <summary>
        /// Adds the parameters to the query
        /// </summary>
        /// <param name="query">The query to execute</param>
        /// <param name="path">The <see cref="BaseItem.EntityId"/> of the requested item</param>
        /// <param name="folderId">The Id of the corresponding <see cref="FolderAssociation.Id"/></param>
        protected abstract void BindSelectByPathQuery(ISQLiteStatement query, string path, TKey folderId);


    }
}
