using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OELib.LibraryBase;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;

namespace OELibTests
{
    [TestClass]
    public class ActorTests
    {
        [TestMethod]
        public void ActorAddTest()
        {
            Actor a = new Actor();
            AutoResetEvent done = new AutoResetEvent(false);
            int i = 0;
            a.Post(() =>
            {
                i = 10;
                done.Set();
            });
            done.WaitOne();
            Assert.AreEqual(10, i);
            a.Dispose();
        }

        [TestMethod]
        public void ActorMultipleAddTest()
        {
            Stopwatch sw = Stopwatch.StartNew();
            Actor a = new Actor();
            int posterCount = 30;
            long itterations = 100000;
            long i = 0;
            AutoResetEvent[] done = new AutoResetEvent[posterCount];
            for (int k = 0; k < posterCount; k++)
            {
                int posterIndex = k;
                done[posterIndex] = new AutoResetEvent(false);
                Task.Run(() =>
                {
                    for (long j = 0; j < itterations; j++)
                        a.Post(() =>
                        {
                            i++;
                        });
                    a.Post(
                        () =>
                        {
                            done[posterIndex].Set();
                        }


                            );
                });
            }
            done.ToList().ForEach(p => p.WaitOne());
            Assert.AreEqual(itterations * posterCount, i);
            a.Dispose();
            sw.Stop();
            long ms = sw.ElapsedMilliseconds;
        }


    }
}
