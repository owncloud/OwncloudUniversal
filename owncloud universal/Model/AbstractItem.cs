using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace owncloud_universal.Model
{
    public class AbstractItem
    {
        public virtual FolderAssociation Association { get; set; }
        public virtual int ID { get; set; }
        public virtual bool IsCollection { get; set; }
        public virtual string ChangeKey { get; set; }
    }
}
