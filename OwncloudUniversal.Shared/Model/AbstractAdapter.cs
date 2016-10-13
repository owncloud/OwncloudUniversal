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
        protected AbstractAdapter(bool isBackgroundSync)
        {
            IsBackgroundSync = isBackgroundSync;
        }

        public bool IsBackgroundSync { get; }
        //gibt das neue item zurück
        public abstract Task<AbstractItem> AddItem(AbstractItem item);

        //gibt das aktualisierte item zurück
        public abstract Task<AbstractItem> UpdateItem(AbstractItem item);

        public abstract Task DeleteItem(AbstractItem item);

        public abstract Task<List<AbstractItem>> GetAllItems(FolderAssociation association);
    }
}
