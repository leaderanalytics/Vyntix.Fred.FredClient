namespace LeaderAnalytics.Vyntix.Fred.FredClient.Tests;

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
        List<Vintage> vintages = (await FredClient.GetVintages(symbol, null, null)).Where(x => x.VintageDate <= endDate).ToList();
        List<Observation> obs = (await FredClient.GetObservations(symbol, vintages.Select(x => x.VintageDate).ToList(), DataDensity.Sparse))
            .Where(x => x.ObsDate <= endDate).ToList();

        Assert.AreEqual(18, vintages.Count);
        Assert.AreEqual(2205, obs.Count);
    }
     
    [Test]
    public async Task CPIAUCSL_Test()
    {
        /* CPIAUCSL repeats an unchanged value accross vintages:
         * Steps to reproduce
         * Download CPIAUCSL from here https://alfred.stlouisfed.org/series/downloaddata?seid=CPIAUCSL
         * Select all vintage dates, output type Observations by vintage date new and changed only.
         * Look at observation period 1991-10-01.  
         * Vintage 1993-02-18 has a value of 137.3
         * Vintage 1993-06-15 has a value of #N/A
         * Vintage 1994-02-17 has a value of 137.3
         * Vintage 1993-06-15 is removed since it has a value of #N/A leaving 
         * the two vintages with an unchanged value.
         */


        string symbol = "CPIAUCSL";
        DateTime endDate = new DateTime(2021, 3, 6);
        List<Vintage> vintageDates = (await FredClient.GetVintages(symbol, null, null)).Where(x => x.VintageDate <= endDate).ToList();
        List<Observation> sparse = (await FredClient.GetObservations(symbol, vintageDates.Select(x => x.VintageDate).ToList(), DataDensity.Sparse))
            .Where(x => x.ObsDate <= endDate).ToList();

        Assert.AreEqual(600, vintageDates.Count);
        Assert.AreEqual(2738, sparse.Count);
        var junk = new VintageComposer().MakeSparse(sparse.Cast<IObservation>().ToList()).ToList();
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
        string symbol = "DFII10";
        DateTime endDate = new DateTime(2020, 12, 31);
        List<Vintage> vintageDates = (await FredClient.GetVintages(symbol, null, null)).Where(x => x.VintageDate <= endDate).ToList();
        List<Observation> sparse = (await FredClient.GetObservations(symbol, vintageDates.Select(x => x.VintageDate).ToList(), DataDensity.Sparse))
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
