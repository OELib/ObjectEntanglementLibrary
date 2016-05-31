using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;


using OELib.LibraryBase;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace OELibTests
{
    [TestClass]
    public class PriorityQueueTest
    {

        [TestMethod]
        public void AddTakeTest()
        {
            var pq = new PriorityQueue<int>();
            int i = 100;
            int j = 0;
            pq.Add(i);
            j = pq.Take();
            Assert.AreEqual(i, j);
        }

        [TestMethod]
        public void CompletedTest()
        {
            var pq = new PriorityQueue<int>();
            pq.Add(1);
            pq.CompleteAdding();
            pq.Take();
            try
            {
                pq.Take();
                Assert.Fail();
            }
            catch (InvalidOperationException) { }
            try
            {
                pq.Add(1);
                Assert.Fail();
            }
            catch (InvalidOperationException) { }
        }
        
        [TestMethod]
        public void OneProducerOneConsumerTest()
        {
            AutoResetEvent finishEvent = new AutoResetEvent(false);
            List<int> collected = new List<int>();
            var pq = new PriorityQueue<int>();
            Task.Run(()=>
            {
                for (int i = 0; i < 1000; i++) pq.Add(i);
                pq.CompleteAdding();
            });

            Task.Run(() =>
            {

                try
                {
                    while (true)
                        collected.Add(pq.Take());
                }
                catch (InvalidOperationException) { }
                finally
                {
                    finishEvent.Set();
                }

            });
            finishEvent.WaitOne();
            Assert.AreEqual(1000, collected.Count);
        }




        [TestMethod]
        public void MultipleProducersMultipleConsumersTest()
        {
            int producers = 10;
            int consumers = 10;
            int produceCount = 1000;


            var pq = new PriorityQueue<long>();

            var consumed = new List<long>[consumers];
            var consumersDoneEvt = new AutoResetEvent[consumers];

            for (int i = 0; i < consumers; i++)
            {
                consumed[i] = new List<long>();
                consumersDoneEvt[i] = new AutoResetEvent(false);
                int index = i;
                Task.Run(() =>
                {
                    try
                    {
                        while (true) consumed[index].Add(pq.Take());
                    }
                    catch (InvalidOperationException) { }
                    finally
                    {
                        consumersDoneEvt[index].Set();
                    }

                });
            }

            var producersDoneEvt = new AutoResetEvent[producers];

            for (int i = 0; i < producers; i++)
            {
                producersDoneEvt[i] = new AutoResetEvent(false);
                int index = i;
                Task.Run(() =>
                {
                    for (int j = 0; j < produceCount; j++) pq.Add(j * (long)(index + 1));
                    producersDoneEvt[index].Set();
                });
            }
            producersDoneEvt.ToList().All(p => p.WaitOne());
            pq.CompleteAdding();
            consumersDoneEvt.ToList().All(p => p.WaitOne());
            Assert.AreEqual(produceCount * producers, consumed.SelectMany(l => l).ToList().Count);
        }








    }
}
