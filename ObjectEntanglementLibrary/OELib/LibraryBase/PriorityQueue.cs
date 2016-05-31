using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OELib.LibraryBase
{
    public class PriorityQueue<T> 
    {
        private bool _completed;
        private List<T> _queue = new List<T>();
        private readonly object _lock = new object();
        
        public void Add(T item)
        {
            lock(_lock)
            {
                if (_completed) throw new InvalidOperationException("Priority queue completed.");
                _queue.Add(item);
                Monitor.Pulse(_lock);
            }
        }

        public void CompleteAdding()
        {
            lock(_lock)
            {
                _completed = true;
                Monitor.PulseAll(_lock);
            }
        }


        public T Take()
        {
            T item;
            lock(_lock)
            {
                while (_queue.Count == 0 && !_completed)
                    Monitor.Wait(_lock);
                if (_completed && _queue.Count == 0) throw new InvalidOperationException("Priority queue completed.");
                item = _queue[0];
                _queue.RemoveAt(0);
            }
            return item;
        }
        
    }
}
