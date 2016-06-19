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

            query = @"CREATE TABLE IF NOT EXISTS [LocalItem] (
                        [Id] INTEGER  NOT NULL PRIMARY KEY AUTOINCREMENT,
                        [AssociationId] INTEGER NOT NULL,
                        [LastModified] TIMESTAMP  NULL,
                        [IsCollection] BOOLEAN  NULL,
                        [Path] TEXT  NULL,
                        FOREIGN KEY(AssociationId) REFERENCES Association(Id)
                        );";
            using (var statement = Connection.Prepare(query))
            {
                statement.Step();
            }

            query = @"CREATE TABLE IF NOT EXISTS [RemoteItem] (
                        [Id] INTEGER  PRIMARY KEY AUTOINCREMENT NOT NULL,
                        [AssociationId] INTEGER NOT NULL,
                        [Etag] NVARCHAR(255)  NULL,
                        [IsCollection] BOOLEAN  NULL,
                        [Href] TEXT  NULL,
                        [DisplayName] NVARCHAR(255)  NULL,
                        [LastModified] TIMESTAMP  NULL,
                        FOREIGN KEY(AssociationId) REFERENCES Association(Id)
                    );";
            using (var statement = Connection.Prepare(query))
            {
                statement.Step();
            }

            query = @"CREATE TABLE IF NOT EXISTS [Association] (
                        [Id] INTEGER  PRIMARY KEY AUTOINCREMENT NOT NULL
                        [IsActive] BOOLEAN  NULL,
                        [SyncDirection] NVARCHAR(32),
                        [LocalPath] TEXT NULL,
                        [RemotePath] TEXT NULL,
                        [ModifyDate] TIMESTAMP NULL,
                        [Etag] NVARCHAR(255)
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
