using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace OwncloudUniversal.Synchronization.Model
{
    /// <summary>
    /// Represents an <see cref="AbstractAdapter"></see> that is able to use the Windows-BackgroundTransfer-API 
    /// </summary>
    public interface IBackgroundSyncAdapter
    {
        AbstractAdapter GetAdapter();
        Task CreateDownload(BaseItem source, IStorageItem targetItem);
    }
}
