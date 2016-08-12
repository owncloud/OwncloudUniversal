using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace owncloud_universal.Model
{
    public class LinkStatus
    {
        public int Id { get; set; }
        public int TargetItemId { get; set; }
        public int SourceItemId { get; set; }
        public int ChangeNumber { get; set; }
        public int AssociationId { get; set; }
    }
}