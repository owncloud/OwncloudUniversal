using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace owncloud_universal.Model
{
    public class LocalItem : AbstractItem
    {
        public DateTime? LastModified { get; set; }        
        public string Path { get; set; }
        public override string ChangeKey
        {
            get
            {
                return Path;
            }

            set
            {
                Path = value;
            }
        }

    }
}
