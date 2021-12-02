using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeaderAnalytics.Vyntix.Fred.Model;
using NUnit.Framework;

namespace LeaderAnalytics.Vyntix.Fred.FredClient.Tests
{
    [TestFixture(FredFileType.XML)]
    public class VintageComposerSymbolTests : BaseTest
    {
        public VintageComposerSymbolTests(FredFileType fileType) : base(fileType)
        { 
        
        }

        [Test]
        public async Task NROU_Test()
        {
            string symbol = "NROU";
            DateTime endDate = new DateTime(2021, 3, 6);
            List<Vintage> vintages = (await FredClient.GetVintageDates(symbol, null)).Where(x => x.VintageDate <= endDate).ToList();
            List<Observation> obs = (await FredClient.GetObservations(symbol, vintages.Select(x => x.VintageDate).ToList()))
                .Where(x => x.ObsDate <= endDate).ToList();

            Assert.AreEqual(18, vintages.Count);
            Assert.AreEqual(2205, obs.Count);
        }

        [Test]
        public async Task CPIAUCSL_Test()
        {
            string symbol = "CPIAUCSL";
            DateTime endDate = new DateTime(2021, 3, 6);
            List<Vintage> vintageDates = (await FredClient.GetVintageDates(symbol, null)).Where(x => x.VintageDate <= endDate).ToList();
            List<Observation> sparse = (await FredClient.GetObservations(symbol, vintageDates.Select(x => x.VintageDate).ToList()))
                .Where(x => x.ObsDate <= endDate).ToList();

            Assert.AreEqual(600, vintageDates.Count);
            Assert.AreEqual(2737, sparse.Count);
            
            // Group sparse observations into vintages
            var vintages = sparse.GroupBy(x => x.VintageDate).ToList();
            
            // Make sure we did not wind up with more vintage dates than was in our original list of vintage dates.
            var missingInVintageDates = vintages.Where(v => !vintageDates.Any(vd => vd.VintageDate == v.Key)).ToList();
            
            // Make sure every date in the original list is included in the output
            var missingInVintages = vintageDates.Where(vd => !vintages.Any(v => v.Key == vd.VintageDate)).ToList();
        }

        [Test]
        public async Task DFII10_Test()
        {
            //
            // FAILS - runs out of threads
            //
            string symbol = "DFII10";
            DateTime endDate = new DateTime(2020, 12, 31);
            List<Vintage> vintageDates = (await FredClient.GetVintageDates(symbol, null)).Where(x => x.VintageDate <= endDate).ToList();
            List<Observation> sparse = (await FredClient.GetObservations(symbol, vintageDates.Select(x => x.VintageDate).ToList()))
                .Where(x => x.ObsDate <= endDate).ToList();

            Assert.AreEqual(1662, vintageDates.Count);
            Assert.AreEqual(4503, sparse.Count);

            // Group sparse observations into vintages
            var vintages = sparse.GroupBy(x => x.VintageDate).ToList();
            Assert.AreEqual(1662, vintages.Count);

            // Make sure we did not wind up with more vintage dates than was in our original list of vintage dates.
            var missingInVintageDates = vintages.Where(v => !vintageDates.Any(vd => vd.VintageDate == v.Key)).ToList();
            Assert.AreEqual(0, missingInVintageDates.Count);

            // Make sure every date in the original list is included in the output
            var missingInVintages = vintageDates.Where(vd => !vintages.Any(v => v.Key == vd.VintageDate)).ToList();
            Assert.AreEqual(0, missingInVintages.Count);
        }
    }
}
