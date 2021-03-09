using LeaderAnalytics.Vyntix.Fred.Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaderAnalytics.Vyntix.Fred.FredClient.Tests
{
    [TestFixture()]
    public class VintageComposerStaticDataTests
    {
        [Test]
        public void VerifyStaticDataTest()
        {
            
            List<Vintage> testData = CreateTestData();
            Assert.AreEqual(3, testData.Count);
            Assert.AreEqual(3, testData[0].Observations.Count);
            Assert.AreEqual(5, testData[1].Observations.Count);
            Assert.AreEqual(6, testData[2].Observations.Count);
        }

        [Test]
        public void MakeSparseTest()
        {
            VintageComposer composer = new VintageComposer();
            List<Vintage> testData = CreateTestData();
            List<IObservation> dense = testData.SelectMany(x => x.Observations).ToList();
            List<IObservation> sparse = composer.MakeSparse(dense);
            Assert.AreEqual(8, sparse.Count);
        }

        [Test]
        public void MakeDenseTest()
        {
            VintageComposer composer = new VintageComposer();
            List<Vintage> testData = CreateTestData();
            List<IObservation> dense = testData.SelectMany(x => x.Observations).ToList();
            List<IObservation> sparse = composer.MakeSparse(dense);
            dense = composer.MakeDense(sparse);
            Assert.AreEqual(14, dense.Count);
        }

       


        private List<Vintage> CreateTestData()
        {
            List<Vintage> vintages = new List<Vintage>();
            vintages.Add(new Vintage { Symbol = "1", VintageDate = new DateTime(2000, 1, 1), Observations = new List<IObservation>() });
            vintages.Add(new Vintage { Symbol = "1", VintageDate = new DateTime(2000, 2, 1), Observations = new List<IObservation>() });
            vintages.Add(new Vintage { Symbol = "1", VintageDate = new DateTime(2000, 3, 1), Observations = new List<IObservation>() });

            Vintage v1 = vintages[0];
            v1.Observations.Add(new Observation { Vintage = v1, ObsDate = new DateTime(1999, 1, 1), Value = "1" });
            v1.Observations.Add(new Observation { Vintage = v1, ObsDate = new DateTime(1999, 2, 1), Value = "2" });
            v1.Observations.Add(new Observation { Vintage = v1, ObsDate = new DateTime(1999, 3, 1), Value = "3" });


            Vintage v2 = vintages[1];
            v2.Observations.Add(new Observation { Vintage = v2, ObsDate = new DateTime(1999, 1, 1), Value = "1" });
            v2.Observations.Add(new Observation { Vintage = v2, ObsDate = new DateTime(1999, 2, 1), Value = "2" });
            v2.Observations.Add(new Observation { Vintage = v2, ObsDate = new DateTime(1999, 2, 2), Value = "2.2" }); //Missing in Vintage 1
            v2.Observations.Add(new Observation { Vintage = v2, ObsDate = new DateTime(1999, 3, 1), Value = "3.5" });
            v2.Observations.Add(new Observation { Vintage = v2, ObsDate = new DateTime(1999, 4, 1), Value = "4" });


            Vintage v3 = vintages[2];
            v3.Observations.Add(new Observation { Vintage = v3, ObsDate = new DateTime(1999, 1, 1), Value = "1" });
            v3.Observations.Add(new Observation { Vintage = v3, ObsDate = new DateTime(1999, 2, 1), Value = "2" });
            v3.Observations.Add(new Observation { Vintage = v3, ObsDate = new DateTime(1999, 2, 2), Value = "2.2" }); // must exist here as it cannot be removed
            v3.Observations.Add(new Observation { Vintage = v3, ObsDate = new DateTime(1999, 3, 1), Value = "3" });
            v3.Observations.Add(new Observation { Vintage = v3, ObsDate = new DateTime(1999, 4, 1), Value = "4" });
            v3.Observations.Add(new Observation { Vintage = v3, ObsDate = new DateTime(1999, 5, 1), Value = "5" });
            return vintages;
        }
    }
}
