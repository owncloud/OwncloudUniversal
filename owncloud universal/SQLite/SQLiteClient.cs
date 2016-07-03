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

            query = @"CREATE TABLE IF NOT EXISTS [SyncItem] (
                        [Id] INTEGER  NOT NULL PRIMARY KEY AUTOINCREMENT,
                        [LastModified] TIMESTAMP  NULL,
                        [IsCollection] BOOLEAN  NULL,
                        [Path] TEXT  NULL,
<<<<<<< .mine
                        [FolderId] INTEGER NULL,
                        [RemoteItemId] INTEGER NULL
||||||| .r7
                        [FolderId] INTEGER NULL
=======
                        [RelativePath] TEXT NULL,
                        [Etag] NVARCHAR(255) NULL,
                        [FolderId] INTEGER NULL
>>>>>>> .r12
                    );";
            using (var statement = Connection.Prepare(query))
            {
                statement.Step();
            }

<<<<<<< .mine
            query = @"CREATE TABLE IF NOT EXISTS [RemoteItem] (
                        [Id] INTEGER  PRIMARY KEY AUTOINCREMENT NOT NULL,
                        [Etag] NVARCHAR(255)  NULL,
                        [IsCollection] BOOLEAN  NULL,
                        [Href] TEXT  NULL,
                        [DisplayName] NVARCHAR(255)  NULL,
                        [LastModified] TIMESTAMP  NULL,
                        [FolderId] INTEGER NULL,
                        [LocalItemId] INTEGER NULL
                    );";
            using (var statement = Connection.Prepare(query))
            {
                statement.Step();
            }

||||||| .r7
            query = @"CREATE TABLE IF NOT EXISTS [RemoteItem] (
                        [Id] INTEGER  PRIMARY KEY AUTOINCREMENT NOT NULL,
                        [Etag] NVARCHAR(255)  NULL,
                        [IsCollection] BOOLEAN  NULL,
                        [Href] TEXT  NULL,
                        [DisplayName] NVARCHAR(255)  NULL,
                        [LastModified] TIMESTAMP  NULL,
                        [FolderId] INTEGER NULL
                    );";
            using (var statement = Connection.Prepare(query))
            {
                statement.Step();
            }

=======
>>>>>>> .r12
            query = @"CREATE TABLE IF NOT EXISTS [Association] (
                        [Id] INTEGER  PRIMARY KEY AUTOINCREMENT NOT NULL,
                        [LocalItemId] INTEGER  NULL,
                        [RemoteItemId] INTEGER  NULL,
                        [IsActive] BOOLEAN  NULL,
                        [SyncDirection] NVARCHAR(32),
                        FOREIGN KEY(LocalItemId) REFERENCES SyncItem(Id),
                        FOREIGN KEY(RemoteItemId) REFERENCES SyncItem(Id)
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
