using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OwncloudUniversal.Synchronization.Processing
{
    public class PauseTokenSource
    {
        private volatile TaskCompletionSource<bool> _paused;
        internal static readonly Task CompletedTask = Task.FromResult(true);
        public PauseToken Token { get { return new PauseToken(this); } }

        public bool IsPaused
        {
            get { return _paused != null; }
            set
            {
                if (value)
                {
                    Interlocked.CompareExchange(
                        ref _paused, new TaskCompletionSource<bool>(), null);
                }
                else
                {
                    while (true)
                    {
                        var tcs = _paused;
                        if (tcs == null) return;
                        if (Interlocked.CompareExchange(ref _paused, null, tcs) == tcs)
                        {
                            tcs.SetResult(true);
                            break;
                        }
                    }
                }
            }
        }

        internal Task WaitWhilePausedAsync()
        {
            var cur = _paused;
            return cur != null ? cur.Task : CompletedTask;
        }

    }
}
