using System;
using System.Threading;
using System.Threading.Tasks;

namespace OELib.LibraryBase
{
    public class Actor : IDisposable
    {
        protected readonly Thread _thread;
        protected PriorityQueue<Action> _inbox = new PriorityQueue<Action>();

        public Actor(ThreadPriority priority = ThreadPriority.Normal)
        {
            _thread = new Thread(loop) {IsBackground = true, Name = GetType().ToString(), Priority = priority};
            _thread.Start();
        }

        public bool Idle => _inbox.Count == 0;

        public void Dispose()
        {
            _inbox.CompleteAdding();
            _thread.Join();
            _inbox.Dispose();
        }

        private void loop()
        {
            foreach (var action in _inbox.GetConsumingEnumerable())
                action();
        }

        public void Post(Action action, Priority priority = Priority.Normal)
        {
            _inbox.Add(action, priority);
        }

        public bool PostWait(Action action, Priority priority = Priority.Normal,
            int timeout = 0) // timeout <= 0 means infinite
        {
            var complete = new AutoResetEvent(false);
            Post(() =>
            {
                action();
                complete.Set();
            }, priority);
            if (timeout > 0) return complete.WaitOne(timeout);
            complete.WaitOne();
            return true;
        }

        public void Clear()
        {
            while (_inbox.Count > 0)
                _inbox.Take();
        }

        public Task PostWaitAsync(Action action, Priority priority = Priority.Normal, int timeout = 0)
        {
            return Task.Run(() => PostWait(action, priority, timeout));
        }
    }
}