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
        List<FredVintage> vintages = (await FredClient.GetVintages(symbol, null, endDate)).ToList();
        List<FredObservation> obs = (await FredClient.GetObservations(symbol, vintages.Select(x => x.VintageDate).ToList(), DataDensity.Sparse))
            .Where(x => x.ObsDate <= endDate).ToList();

        Assert.AreEqual(18, vintages.Count);
        Assert.AreEqual(2205, obs.Count);
    }
     
    [Test]
    public async Task CPIAUCSL_Test()
    {
        /* ALFRED spreadsheet for CPIAUCSL repeats an unchanged value accross vintages:
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

        IVintageComposer composer = new VintageComposer();
        string symbol = "GDPC1";
        DateTime startDate = new DateTime(2019, 1, 1);
        DateTime endDate = new DateTime(2019, 12, 31);
        List<DateTime> vintageDates = (await FredClient.GetVintageDates(symbol, startDate, endDate)).ToList();
        // Observations as returned by FRED 
        List<FredObservation> sparseNative = (await ((BaseFredClient)FredClient).GetObservationsInternal(symbol, vintageDates, startDate, endDate, DataDensity.Sparse));
        // Afer MakeSparse is called 
        List<FredObservation> sparseComposed = (await FredClient.GetObservations(symbol, vintageDates, startDate, endDate, DataDensity.Sparse));

        
        Assert.AreEqual(11, vintageDates.Count);
        Assert.AreEqual(sparseNative.Count, sparseComposed.Count);
        
        // Group sparse observations into vintages
        var vintages = sparseComposed.GroupBy(x => x.VintageDate).ToList();

        // Make sure we did not wind up with more vintage dates than was in our original list of vintage dates.
        var missingInVintageDates = vintages.Where(v => !vintageDates.Any(vd => vd == v.Key));
        Assert.AreEqual(0, missingInVintageDates.Count());

        // Make sure every date in the original list is included in the output
        var missingInVintages = vintageDates.Where(vd => !vintages.Any(v => v.Key == vd));
        Assert.AreEqual(2, missingInVintages.Count()); // Two vintages are returned which have no observations within start / end dates

        List<IFredObservation> doubleSparse = composer.MakeSparse(sparseComposed.Cast<IFredObservation>().ToList()).ToList();
        Assert.AreEqual(sparseComposed.Count, doubleSparse.Count);

        // convert sparse to dense using VintageComposer
        List<IFredObservation> denseComposed = composer.MakeDense(sparseComposed.Cast<IFredObservation>().ToList()).ToList();

        // Get a list of dense observations natively from FRED
        List<FredObservation> denseNative = (await ((BaseFredClient)FredClient).GetObservationsInternal(symbol, vintageDates, startDate, endDate, DataDensity.Dense)).ToList();

        Assert.AreEqual(denseNative.Count, denseComposed.Count);
    }

    [Test]
    public async Task DFII10_Test()
    {
        string symbol = "DFII10";
        IVintageComposer composer = new VintageComposer();
        DateTime endDate = new DateTime(2020, 12, 31);
        List<DateTime> vintageDates = (await FredClient.GetVintageDates(symbol, null, endDate)).ToList();
        
        // Observations as returned by FRED
        List<FredObservation> sparseNative = (await ((BaseFredClient)FredClient).GetObservationsInternal(symbol, vintageDates, null, endDate, DataDensity.Sparse)).ToList();
        // Afer MakeSparse is called
        List<FredObservation> sparseComposed = (await FredClient.GetObservations(symbol, vintageDates, null, endDate, DataDensity.Sparse)).ToList();

        Assert.AreEqual(3676, vintageDates.Count);
        Assert.AreEqual(sparseNative.Count, sparseComposed.Count);

        // Group sparse observations into vintages
        var vintages = sparseComposed.GroupBy(x => x.VintageDate).ToList();
        Assert.AreEqual(3676, vintages.Count);

        // Make sure we did not wind up with more vintage dates than was in our original list of vintage dates.
        var missingInVintageDates = vintages.Where(v => !vintageDates.Any(vd => vd == v.Key)).ToList();
        Assert.AreEqual(0, missingInVintageDates.Count);

        // Make sure every date in the original list is included in the output
        var missingInVintages = vintageDates.Where(vd => !vintages.Any(v => v.Key == vd)).ToList();
        Assert.AreEqual(0, missingInVintages.Count);
        
        // convert sparse to dense using VintageComposer
        List<IFredObservation> denseComposed = composer.MakeDense(sparseComposed.Cast<IFredObservation>().ToList()).ToList();
        
        // Get a list of dense observations natively from FRED
        List<FredObservation> denseNative = (await ((BaseFredClient)FredClient).GetObservationsInternal(symbol, vintageDates, null, endDate, DataDensity.Dense)).ToList();
        
        Assert.AreEqual(denseNative.Count, denseComposed.Count);

    }
}
