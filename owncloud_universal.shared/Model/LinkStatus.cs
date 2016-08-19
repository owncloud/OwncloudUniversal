using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwncloudUniversal.Shared.Model
{
    public class LinkStatus
    {
        public LinkStatus() { }
        public LinkStatus(AbstractItem sourceItem, AbstractItem targetItem)
        {
            SourceItemId = sourceItem.Id;
            TargetItemId = targetItem.Id;
            ChangeNumber = 0;
            AssociationId = sourceItem.Association.Id;
        }
        public long Id { get; set; }
        public long TargetItemId { get; set; }
        public long SourceItemId { get; set; }
        public long ChangeNumber { get; set; }
        public long AssociationId { get; set; }
    }
}