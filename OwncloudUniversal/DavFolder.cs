using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using OwncloudUniversal.Shared;
using OwncloudUniversal.Shared.LocalFileSystem;
using OwncloudUniversal.Shared.Model;
using OwncloudUniversal.WebDav;

namespace OwncloudUniversal
{
    public class DavFolder : INotifyPropertyChanged
    {
        private Uri _href;
        public Uri Href
        {
            get { return _href; }
            set
            {
                _href = value;
                LoadItems();
                OnPropertyChanged();
            }
        }

        public string FolderName
        {
            get
            {
                var serverUri = new Uri(Configuration.ServerUrl);
                var itemUri = new Uri(serverUri, Href);
                var name = WebUtility.UrlDecode("/" + serverUri.MakeRelativeUri(itemUri));
                if (name.Length > 40)
                {
                    name = "..." + name.Substring(name.Length - 40);
                }
                return name;
            }
        }

        private List<DavItem> _items;
        public List<DavItem> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                OnPropertyChanged();
            }
        }
        private WebDavAdapter DavAdapter { get;}
        private FileSystemAdapter FileSystemAdapter{ get;}
        
        public DavFolder()
        {
            FileSystemAdapter = new FileSystemAdapter(false, null);
            DavAdapter = new WebDavAdapter(false, Configuration.ServerUrl, Configuration.Credential, FileSystemAdapter);
            FileSystemAdapter.LinkedAdapter = DavAdapter;
        }

        private async void LoadItems()
        {
            var list = await DavAdapter.GetAllItems(CreateItemUri());
            list.Remove(list[0]);
            Items = list.OrderBy(x => !x.IsCollection).ThenBy(x => ((DavItem)x).DisplayName).Cast<DavItem>().ToList();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private Uri CreateItemUri()
        {
            if (string.IsNullOrWhiteSpace(Configuration.ServerUrl))
                return null;
            var serverUri = new Uri(Configuration.ServerUrl, UriKind.RelativeOrAbsolute);
            return new Uri(serverUri, Href);
        }
    }
}
