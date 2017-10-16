using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace OELib.LibraryBase
{
    public class PriorityQueue<T> : IProducerConsumerCollection<T>, IDisposable
    {
        private readonly object _lock = new object();
        private readonly LinkedList<T> _queue = new LinkedList<T>();
        private bool _completed;

        public void Dispose()
        {
            CompleteAdding();
        }

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _queue.Count;
                }
            }
        }

        public bool IsSynchronized => true;

        public object SyncRoot => throw new NotImplementedException();

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int index)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            T item;
            while (TryTake(out item))
                yield return item;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            T item;
            while (TryTake(out item))
                yield return item;
        }

        public T[] ToArray()
        {
            throw new NotImplementedException();
        }

        public bool TryAdd(T item)
        {
            return TryAdd(item, Priority.Normal);
        }

        public bool TryTake(out T item)
        {
            lock (_lock)
            {
                while (_queue.Count == 0 && !_completed)
                    Monitor.Wait(_lock);
                if (_completed && _queue.Count == 0)
                {
                    item = default(T);
                    return false;
                }
                item = _queue.First.Value;
                _queue.RemoveFirst();
                return true;
            }
        }

        public void Add(T item)
        {
            if (!TryAdd(item)) throw new InvalidOperationException("Priority queue completed.");
        }

        public void Add(T item, Priority priority)
        {
            if (!TryAdd(item, priority)) throw new InvalidOperationException("Priority queue completed.");
        }

        public void CompleteAdding()
        {
            lock (_lock)
            {
                _completed = true;
                Monitor.PulseAll(_lock);
            }
        }

        public IEnumerable<T> GetConsumingEnumerable()
        {
            T item;
            while (TryTake(out item))
                yield return item;
        }

        public T Take()
        {
            T item;
            if (!TryTake(out item)) throw new InvalidOperationException("Priority queue completed.");
            return item;
        }

        public bool TryAdd(T item, Priority priority)
        {
            lock (_lock)
            {
                if (_completed) return false;
                switch (priority)
                {
                    case Priority.Normal:
                        _queue.AddLast(item);
                        break;
                    case Priority.High:
                        _queue.AddFirst(item);
                        break;
                }
                Monitor.Pulse(_lock);
                return true;
            }
        }
    }
}