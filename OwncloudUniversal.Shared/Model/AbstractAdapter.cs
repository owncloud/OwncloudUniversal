using System;
using System.Collections.Generic;
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
        public abstract Task<AbstractItem> AddItem(AbstractItem item);

        //gibt das aktualisierte item zurück
        public abstract Task<AbstractItem> UpdateItem(AbstractItem item);//TODO only use entityid?

        public abstract Task DeleteItem(AbstractItem item);

        public abstract Task<AbstractItem> GetItem(string entityId);

        public abstract Task<List<AbstractItem>> GetAllItems(FolderAssociation association);
    }
}
