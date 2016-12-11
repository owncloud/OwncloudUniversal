using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OwncloudUniversal.Shared;
using OwncloudUniversal.Shared.LocalFileSystem;
using OwncloudUniversal.Shared.Model;
using OwncloudUniversal.WebDav;

namespace OwncloudUniversal.Services
{
    class WebDavItemService
    {
        private static WebDavItemService _instance;
        private WebDavItemService()
        {
            FileSystemAdapter = new FileSystemAdapter(false, null);
            DavAdapter = new WebDavAdapter(false, Configuration.ServerUrl, Configuration.Credential, FileSystemAdapter);
            FileSystemAdapter.LinkedAdapter = DavAdapter;
        }

        public static WebDavItemService GetDefault()
        {
            return _instance ?? (_instance = new WebDavItemService());
        }

        private WebDavAdapter DavAdapter { get; }
        private FileSystemAdapter FileSystemAdapter { get; }

        public async Task<List<AbstractItem>> GetItemsAsync(Uri folderHref)
        {
            return await DavAdapter.GetAllItems(CreateItemUri(folderHref));
        }

        private Uri CreateItemUri(Uri href)
        {
            if (string.IsNullOrWhiteSpace(Configuration.ServerUrl))
                return null;
            var serverUri = new Uri(Configuration.ServerUrl, UriKind.RelativeOrAbsolute);
            return new Uri(serverUri, href);
        }

        public async Task UploadItemAsync(AbstractItem itemToUpload, string targetFolderHref)
        {
            await DavAdapter.AddItemAsync(itemToUpload, targetFolderHref);
        }

    }
}
