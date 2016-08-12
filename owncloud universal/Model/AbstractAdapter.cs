using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace owncloud_universal.Model
{
    abstract class AbstractAdapter
    {
        public abstract void AddItem(AbstractItem item);

        public abstract void UpdateItem(AbstractItem item);

        public abstract void DeleteItem(AbstractItem item);

        public abstract Task<List<AbstractItem>> GetAllItems(FolderAssociation association);
    }
}
