using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.Storage.Pickers.Provider;
using Windows.System;
using Windows.UI.Popups;
using OwncloudUniversal.Shared;
using OwncloudUniversal.Shared.LocalFileSystem;
using OwncloudUniversal.Shared.Model;
using OwncloudUniversal.Shared.Synchronisation;
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

        public async Task Upload()
        {
            FileOpenPicker picker = new FileOpenPicker();

            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".odt");
            picker.FileTypeFilter.Add(".ods");
            picker.FileTypeFilter.Add(".odp");
            picker.FileTypeFilter.Add(".doc");
            picker.FileTypeFilter.Add(".docx");
            picker.FileTypeFilter.Add(".mp4");
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".zip");
            picker.FileTypeFilter.Add(".rar");
            picker.FileTypeFilter.Add(".txt");
            picker.FileTypeFilter.Add(".pdf");
            picker.FileTypeFilter.Add(".odf");
            picker.FileTypeFilter.Add(".kdbx");
            picker.FileTypeFilter.Add(".ppt");
            picker.FileTypeFilter.Add(".pptx");
            picker.FileTypeFilter.Add(".xlsx");
            picker.FileTypeFilter.Add(".xls");
            picker.FileTypeFilter.Add(".gif");
            picker.FileTypeFilter.Add(".gz");
            picker.FileTypeFilter.Add(".mid");
            picker.FileTypeFilter.Add(".aac");
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            var files = await picker.PickMultipleFilesAsync();
            if(files.Count == 0) return;
            foreach (var item in await _BuildRemoteItems(files.ToList()))
            {
                await DavAdapter.AddItemAsync(item, Href.ToString());
            }
            MessageDialog dia = new MessageDialog("Upload finished.");
            await dia.ShowAsync();
        }

        public async Task Download(AbstractItem itemToDownload)
        {
            FileSavePicker picker = new FileSavePicker();
            picker.SuggestedStartLocation = PickerLocationId.Downloads;
            picker.SuggestedFileName = Path.GetFileName(itemToDownload.EntityId);
            picker.DefaultFileExtension = Path.GetExtension(itemToDownload.EntityId);
            picker.FileTypeChoices.Add(Path.GetExtension(itemToDownload.EntityId), new List<string> { Path.GetExtension(itemToDownload.EntityId)});
            var storageFile = await picker.PickSaveFileAsync();
            await FileSystemAdapter.AddItem(itemToDownload, storageFile);
            await Launcher.LaunchFileAsync(storageFile);
        }

        private async Task<List<AbstractItem>> _BuildRemoteItems(List<StorageFile> files)
        {
            List<AbstractItem> result = new List<AbstractItem>();
            foreach (var storageFile in files)
            {
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("uploadFile", storageFile);
                var props = await storageFile.GetBasicPropertiesAsync();
                
                LocalItem item = new LocalItem(null, storageFile,props);
                result.Add(item);
            }
            return result;
        }
    }
}
