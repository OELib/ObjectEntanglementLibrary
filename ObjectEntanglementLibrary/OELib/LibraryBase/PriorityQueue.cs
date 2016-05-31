using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace OELib.LibraryBase
{
    public class PriorityQueue<T> : IProducerConsumerCollection<T>
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

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
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

        public bool TryAdd(T item)
        {
            lock (_lock)
            {
                if (_completed) return false;
                _queue.Add(item);
                return true;
            }
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