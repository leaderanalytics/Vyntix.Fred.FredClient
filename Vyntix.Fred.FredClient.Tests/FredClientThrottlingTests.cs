using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeaderAnalytics.Vyntix.Fred.FredClient;
using NUnit.Framework;
using LeaderAnalytics.Vyntix.Fred.Model;
using System.Collections.Concurrent;

namespace LeaderAnalytics.Vyntix.Fred.FredClient.Tests
{
    [TestFixture(FredFileType.JSON)]
    [TestFixture(FredFileType.XML)]
    public class FredClientThrottlingTests : BaseTest
    {

        public FredClientThrottlingTests(FredFileType fileType) : base(fileType)
        {

        }

        [Test()]
        public async Task Download_five_symbols()
        {
            
            string[] symbols = new string[] { "LEU0252881600Q", "CPIAUCSL", "GDP", "M2V", "BAA10Y" };
            DateTime startDate = new DateTime(2000, 1, 1);
            DateTime endDate = new DateTime(2000, 12, 31);
            ConcurrentBag<Observation> observations = new ConcurrentBag<Observation>();
            Task[] tasks = new Task[symbols.Length];

            for (int i = 0; i < symbols.Length; i++)
                tasks[i] = FredClient.GetObservationUpdates(symbols[i], startDate, endDate)
                    .ContinueWith(x => x.Result.ForEach(o => observations.Add(o)));

            Task result = Task.WhenAll(tasks);
            await result;
            Assert.IsFalse(result.IsFaulted);
            Assert.AreEqual(5, observations.GroupBy(x => x.Symbol).Count());
        }
    }
}