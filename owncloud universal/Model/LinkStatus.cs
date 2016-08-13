using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace owncloud_universal.Model
{
    public class LinkStatus
    {
        public long Id { get; set; }
        public long TargetItemId { get; set; }
        public long SourceItemId { get; set; }
        public long ChangeNumber { get; set; }
        public long AssociationId { get; set; }
    }
}