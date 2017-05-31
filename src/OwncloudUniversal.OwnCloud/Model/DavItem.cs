using System;
using Windows.UI.Xaml.Media.Imaging;
using OwncloudUniversal.Shared.Model;

namespace OwncloudUniversal.WebDav.Model
{
    public class DavItem : BaseItem
    {
        public string Href { get; set; }
        public string Etag { get; set; }
        public string ContentType { get; set; }
        public override string DisplayName { get; set; }
        public override string EntityId
        {
            get
            {
                return Href;
            }

            set
            {
                Href = value;
            }
        }
        public override string ChangeKey
        {
            get
            {
                return Etag;
            }

            set
            {
                Etag = value;
            }
        }
        public string ThumbnailUrl { get; set; }
        public override Type AdapterType => typeof(WebDavAdapter);
    }
}
