using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OwncloudUniversal.Shared;
using OwncloudUniversal.Shared.LocalFileSystem;
using OwncloudUniversal.Shared.Model;
using OwncloudUniversal.WebDav;

namespace OwncloudUniversal
{
    public class FolderHelper
    {
        public string Href { get; set; }

        private static FolderHelper _instance;

        public static FolderHelper GetInstance()
        {
            return _instance ?? (_instance = new FolderHelper());
        }
        private FolderHelper()
        {
            FileSystemAdapter = new FileSystemAdapter(false, null);
            DavAdapter = new WebDavAdapter(false, Configuration.ServerUrl, Configuration.Credential, FileSystemAdapter);
            FileSystemAdapter.LinkedAdapter = DavAdapter;
        }
        private WebDavAdapter DavAdapter { get;}
        private FileSystemAdapter FileSystemAdapter{ get;}
        public async Task<List<AbstractItem>>  LoadItems()
        {
            return await DavAdapter.GetAllItems(new Uri(Href));
        }
        
    }
}
