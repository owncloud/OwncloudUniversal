using System;
using System.IO;
using Windows.UI.Xaml.Controls;

namespace OwncloudUniversal.Shared.Model
{
    /// <summary>
    /// The base class for all entities that will be synchronized. Each Entity must have a corresponding <see cref="AbstractAdapter"/>
    /// </summary>
    public class BaseItem
    { 
        /// <summary>
        /// The PrimaryKey of the item in the SQLite-Database
        /// </summary>
        public virtual long Id { get; set; }

        /// <summary>
        /// The Folder or Collection this item is a sub-item of
        /// </summary>
        public virtual FolderAssociation Association { get; set; }

        /// <summary>
        /// The unique identifier of the entity that is used in the corresponding system. (for example a file path)
        /// </summary>
        public virtual string EntityId { get; set; }

        /// <summary>
        /// Indicates wether the item can contain more items (like a directory)
        /// </summary>
        public virtual bool IsCollection { get; set; }

        /// <summary>
        /// Represents the current state of an item.
        /// </summary>
        public virtual string ChangeKey { get; set; }//wenn sich changekey ändert changenum erhöhen

        /// <summary>
        /// Represents the status of the <see cref="ChangeKey"/>. If the <see cref="ChangeKey"/> changes the sync-process must increment the <see cref="ChangeNumber"/>
        /// </summary>
        public virtual long ChangeNumber { get; set; }

        /// <summary>
        /// Represents the amount of data of the item
        /// </summary>
        public virtual ulong Size { get; set; }

        /// <summary>
        /// Synchronisation of items can be postponed if the synch would exceed the available time of a background-task
        /// </summary>
        public virtual bool SyncPostponed{ get; set; }

        /// <summary>
        /// Represents a <see cref="Stream"/> containing the actual data of the item
        /// </summary>
        public virtual  Stream ContentStream { get; set; }

        /// <summary>
        /// Represents the <see cref="Type"/> of the <see cref="AbstractAdapter"/> this item belongs to
        /// </summary>
        public virtual Type AdapterType { get; set; }

        /// <summary>
        /// Represents the display name of an item
        /// </summary>
        public virtual string DisplayName { get; set; }

        /// <summary>
        /// Contains the Date of the last time this item was modified
        /// </summary>
        public virtual DateTime? LastModified { get; set; }

        /// <summary>
        /// Indicates wether this item is in the database and will be (or already was) synchronized.
        /// </summary>
        public bool IsSynced => ItemTableModel.GetDefault().GetItemFromEntityId(this.EntityId) != null;
    }
}
