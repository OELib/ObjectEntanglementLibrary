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
        private bool _completed;
        private List<T> _queue = new List<T>();
        public int Count
        {
            get
            {
                lock (_lock) return _queue.Count;
            }
        }

        public bool IsSynchronized => true;

        public object SyncRoot
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Add(T item)
        {
            if (!TryAdd(item)) throw new InvalidOperationException("Priority queue completed.");
        }
        public void Add(T item, uint priority)
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

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int index)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetConsumingEnumerable()
        {
            T item;
            while (TryTake(out item))
                yield return item;
        }
        public void Dispose()
        {
            CompleteAdding();
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
        public T Take()
        {
            T item;
            if (!TryTake(out item)) throw new InvalidOperationException("Priority queue completed.");
            return item;
        }
        public T[] ToArray()
        {
            throw new NotImplementedException();
        }

        public bool TryAdd(T item, uint priority)
        {
            lock (_lock)
            {
                if (_completed) return false;
                int position = (int)Math.Round((double)Math.Min(100, priority) / 100 * _queue.Count);
                _queue.Insert(position, item);
                Monitor.Pulse(_lock);
                return true;
            }
        }

        public bool TryAdd(T item) => TryAdd(item, 100);

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
                else
                {
                    item = _queue[0];
                    _queue.RemoveAt(0);
                    return true;
                }
            }
        }
    }
}