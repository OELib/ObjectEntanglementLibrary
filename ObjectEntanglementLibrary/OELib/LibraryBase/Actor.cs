using System;
using System.Threading;
using System.Threading.Tasks;

namespace OELib.LibraryBase
{
    public class Actor : IDisposable
    {
        protected PriorityQueue<Action> _inbox = new PriorityQueue<Action>();

        protected readonly System.Threading.Thread _thread;

        public Actor(ThreadPriority priority = ThreadPriority.Normal)
        {
            _thread = new Thread(new ThreadStart(loop)) { IsBackground = true, Name = this.GetType().ToString(), Priority = priority };
            _thread.Start();
        }

        private void loop()
        {
            foreach (Action action in _inbox.GetConsumingEnumerable())
                action();
        }

        public void Post(Action action, Priority priority = Priority.Normal)
        {
            _inbox.Add(action, priority);
        }

        public bool PostWait(Action action, Priority priority = Priority.Normal, int timeout = 0) // timeout <= 0 means infinite
        {
            AutoResetEvent complete = new AutoResetEvent(false);
            this.Post(() =>
            {
                action();
                complete.Set();
            }, priority);
            if (timeout > 0) return complete.WaitOne(timeout);
            else complete.WaitOne();
            return true;
        }

        public void Clear()
        {
            while (_inbox.Count > 0)
                _inbox.Take();
        }

        public bool Idle => _inbox.Count == 0;

        public Task PostWaitAsync(Action action, Priority priority = Priority.Normal, int timeout = 0) => Task.Run(() => PostWait(action, priority, timeout));

        public void Dispose()
        {
            _inbox.CompleteAdding();
            _thread.Join();
            _inbox.Dispose();
        }
    }
}