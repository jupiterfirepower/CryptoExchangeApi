using System;
using System.Threading;

namespace Common.Contracts
{
    public class ApiBase
    {
        private readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        private volatile bool _running = false;

        protected void WaitResourceFreeSignal()
        {
            Thread.MemoryBarrier();
            if (_running)
            {
                Thread.MemoryBarrier();
                try
                {
                    _autoResetEvent.WaitOne(3000); // 3 sec
                }
                catch(ObjectDisposedException)
                {
                }
                catch (ArgumentOutOfRangeException)
                {
                }
                catch (AbandonedMutexException)
                {
                }
                catch (InvalidOperationException)
                {
                }
                finally
                {
                    _running = true;
                }
            }
        }

        protected void AutoResetEventSet()
        {
            Thread.MemoryBarrier();
            if (_running)
            {
                Thread.MemoryBarrier();
                try
                {
                    _autoResetEvent.Set();
                }
                catch (Exception)
                {
                }
                finally
                {
                    _running = false;
                }
            }
        }
    }
}
