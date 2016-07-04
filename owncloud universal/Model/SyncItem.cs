using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace owncloud_universal.Model
{
    public class SyncItem
    {
        public long Id { get; set; }
        public long FolderId { get; set; }
        public DateTime? LastModified { get; set; }
        public bool IsCollection { get; set; }
        public string Path { get; set; }
        public string Etag { get; set; }
        public string RelativePath { get; set; }
        public string DisplayName { get; set; }
        public string GetRelavtivePath(string basePath)
        {
            return this.Path.Remove(0, basePath.Length);
        }
    }
}
