using System.Collections.Generic;
using OwncloudUniversal.Synchronization.Model;

namespace OwncloudUniversal.Synchronization.Processing
{
    class EnityIdComparer : IEqualityComparer<BaseItem>
    {
        public bool Equals(BaseItem x, BaseItem y)
        {
            return x?.EntityId == y?.EntityId;
        }

        public int GetHashCode(BaseItem obj)
        {
            return obj.EntityId.GetHashCode();
        }
    }
}
