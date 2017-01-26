using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OwncloudUniversal.Shared.Model;

namespace OwncloudUniversal.Shared.Model
{
    public abstract class AbstractAdapter
    {
        private bool isBackgroundSync;

        protected AbstractAdapter(bool isBackgroundSync, AbstractAdapter linkedAdapter)
        {
            IsBackgroundSync = isBackgroundSync;
            LinkedAdapter = linkedAdapter;
        }


        public AbstractAdapter LinkedAdapter { get; set; }

        protected bool IsBackgroundSync { get; }

        //gibt das neue item zurück
        public abstract Task<BaseItem> AddItem(BaseItem item);

        //gibt das aktualisierte item zurück
        public abstract Task<BaseItem> UpdateItem(BaseItem item);//TODO only use entityid?

        public abstract Task DeleteItem(BaseItem item);

        public abstract Task<List<BaseItem>> GetUpdatedItems(FolderAssociation association);

        public abstract Task<List<BaseItem>> GetDeletedItemsAsync(FolderAssociation association);

        public abstract string BuildEntityId(BaseItem item);
    }
}
