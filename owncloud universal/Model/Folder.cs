using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using owncloud_universal.WebDav;


namespace owncloud_universal.Model
{
    class Folder
    {
        public bool HasParent { get; set; }
        public string Parent { get; set; }
        public string Href { get; set; }
        public string DisplayName { get; set; }

        public Folder(string href)
        {
            Href = href;
            GetDisplayName();
        }

        public async Task<List<RemoteItem>>  LoadItems()
        {
            var list = new List<RemoteItem>();
            var i = await ConnectionManager.GetFolder(Href);
            list.AddRange(i);
            return list;
        }

        private void GetDisplayName()
        {
            DisplayName = "/";
            var path = Href.Remove(0, Configuration.FolderPath.Length);
            if (!String.IsNullOrWhiteSpace(path) && path.Substring(0, 1) != "/")
                DisplayName = path;
        }
    }
}
