using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OwncloudUniversal.Shared.Model;

namespace OwncloudUniversal.Shared.Model
{
    /// <summary>
    /// Represents the connection of two items that have been synched
    /// </summary>
    public class LinkStatus
    {
        /// <summary>
        /// Intitializes a new instance of a <see cref="LinkStatus"/>
        /// </summary>
        public LinkStatus() { }

        /// <summary>
        /// Intitializes a new instance of a <see cref="LinkStatus"/> and set all properties except <see cref="Id"/>. The <see cref="ChangeNumber"/> is set to zero.
        /// </summary>
        public LinkStatus(BaseItem sourceItem, BaseItem targetItem)
        {
            SourceItemId = sourceItem.Id;
            TargetItemId = targetItem.Id;
            ChangeNumber = 0;
            AssociationId = sourceItem.Association.Id;
        }

        /// <summary>
        /// The Id of the corresponding row in the table of the database
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// The <see cref="BaseItem.Id"/> of the item of the target system.
        /// </summary>
        public long TargetItemId { get; set; }

        /// <summary>
        /// The <see cref="BaseItem.Id"/> of the item of the source system.
        /// </summary>
        public long SourceItemId { get; set; }

        /// <summary>
        /// The number of how often one of those items has been updated
        /// </summary>
        public long ChangeNumber { get; set; }

        /// <summary>
        /// The Id of the corresponding <see cref="FolderAssociation.Id"/>
        /// </summary>
        public long AssociationId { get; set; }
    }
}