﻿using owncloud_universal.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLitePCL;

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
            query.Bind(4, item.AssocaitionId);
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
            query.Bind(4, item.AssocaitionId);
            query.Bind(5, key);
        }

        protected override LocalItem CreateInstance(ISQLiteStatement query)
        {
            LocalItem i = new LocalItem
            {
                Id = (long) query[0],
                LastModified = Convert.ToDateTime(query[1]),
                IsCollection = (long)query[2] == 1,
                Path = (string)query[3],
                AssocaitionId = (long)query[4]
            };
            return i;
        }

        protected override string GetDeleteItemQuery()
        {
            return "DELETE FROM LocalItem WHERE Id = ?";
        }

        protected override string GetInsertItemQuery()
        {
            return "INSERT INTO LocalItem (LastModified, IsCollection, Path, AssiciationId) VALUES (@lastmodified, @iscollection, @path, @associationid)";
        }

        protected override string GetSelectAllQuery()
        {
            return "SELECT Id, LastModified, IsCollection, Path, AssiciationId  FROM LocalItem";
        }

        protected override string GetSelectItemQuery()
        {
            return "SELECT Id, LastModified, IsCollection, Path, AssiciationId FROM LocalItem WHERE Id = ?";
        }

        protected override string GetUpdateItemQuery()
        {
            return "UPDATE LocalItem SET LastModified = ?, IsCollection = ?, Path = ?, AssiciationId = ? WHERE Id= ?";
        }

        protected override string GetLastInsertRowIdQuery()
        {
            return "SELECT last_insert_rowid() FROM LocalItem";
        }
    }
}
