using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace owncloud_universal.Model
{
    class LocalItem
    {
        public long Id { get; set; }
        public long AssocaitionId { get; set; }
        public DateTime? LastModified { get; set; }
        public bool IsCollection { get; set; }
        public string Path { get; set; }
    }
}
