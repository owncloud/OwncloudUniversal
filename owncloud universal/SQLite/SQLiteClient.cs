using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLitePCL;

namespace owncloud_universal
{
    public static class SQLiteClient
    {
        public static SQLiteConnection Connection;
        public static void Init()
        {
            Connection = new SQLiteConnection("webdav-sync.db");
            string query = "";

            query = @"CREATE TABLE IF NOT EXISTS [Item] (
                        [Id] INTEGER  NOT NULL PRIMARY KEY AUTOINCREMENT,
                        [FolderId] INTEGER NULL,
                        [IsCollection] BOOLEAN  NULL,
                        [ChangeKey] TEXT  NULL,
                        [ItemId] Text NULL
                    );";
            using (var statement = Connection.Prepare(query))
            {
                statement.Step();
            }

            query = @"CREATE TABLE IF NOT EXISTS [Association] (
                        [Id] INTEGER  PRIMARY KEY AUTOINCREMENT NOT NULL,
                        [LocalItemId] INTEGER  NULL,
                        [RemoteItemId] INTEGER  NULL,
                        [IsActive] BOOLEAN  NULL,
                        [SyncDirection] NVARCHAR(32),
                        FOREIGN KEY(LocalItemId) REFERENCES LocalItem(Id),
                        FOREIGN KEY(RemoteItemId) REFERENCES RemoteItem(Id)
                    );";
            using (var statement = Connection.Prepare(query))
            {
                statement.Step();
            }

            //activate foreign keys?
            query = "PRAGMA foreign_keys = ON";
            using (var statement = Connection.Prepare(query))
            {
                statement.Step();
            }

        }
    }
}
