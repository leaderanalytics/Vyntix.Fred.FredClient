namespace LeaderAnalytics.Vyntix.Fred.FredClient.Tests;

[TestFixture(FredFileType.JSON)]
//[TestFixture(FredFileType.XML)]
public class FredClientThrottlingTests : BaseTest
{

    public FredClientThrottlingTests(FredFileType fileType) : base(fileType)
    {

    }

    [Test]
    public async Task Download_five_symbols()
    {
        string[] symbols = new string[] { "LEU0252881600Q", "CPIAUCSL", "GDP", "M2V", "BAA10Y" };
        DateTime startDate = new DateTime(2020, 1, 1);
        DateTime endDate = new DateTime(2020, 12, 31);
        ConcurrentBag<FredObservation> observations = new ConcurrentBag<FredObservation>();
        Task[] tasks = new Task[symbols.Length];

        for (int i = 0; i < symbols.Length; i++)
            tasks[i] = FredClient.GetObservations(symbols[i], startDate, endDate, DataDensity.Dense)
                .ContinueWith(x => x.Result.Data.ForEach(o => observations.Add(o)));

        Task result = Task.WhenAll(tasks);
        await result;
        Assert.IsFalse(result.IsFaulted);
        Assert.AreEqual(5, observations.GroupBy(x => x.Symbol).Count());
    }

    [Test]
    public async Task vintage_loop_test()
    {
        // 2015-11-27 is reported as a vintage but actually contains no values:
        //observation_date BAA10Y_20151125 BAA10Y_20151127 BAA10Y_20151130
        //2015 - 11 - 24   3.20
        //2015 - 11 - 25    	           #N/A	
        //2015 - 11 - 26	    	       #N/A	
        //2015 - 11 - 27                                   3.22

        DateTime cutoff = new DateTime(2022, 12, 1);
        List<DateTime> vintageDates = (await FredClient.GetVintageDates("BAA10Y", null, cutoff)).Data;
        List<FredObservation> data = (await FredClient.GetObservations("BAA10Y", vintageDates, DataDensity.Sparse)).Data; 
        
        int dataVintageCount = data.GroupBy(x => x.VintageDate).Count();                            // Includes 2015-11-27
        List<DateTime> dataVintageDates = data.Select(x => x.VintageDate).ToList();                 // Does not include 2015-11-27
        

        Assert.AreEqual(dataVintageCount +1 , vintageDates.Count());
    }


    [Test]
    public async Task EnduranceTest()
    {
        return; // takes too long

        for (int i = 0; i < 10; i++)
        {
            List<FredReleaseDate> dates = await FredClient.GetAllReleaseDates(null, true);

            if (dates is null)
                throw new Exception("dates is null");


            if (dates?.Any() ?? false)
            {
                foreach (var grp in dates.GroupBy(x => x.ReleaseID))
                {
                    FredRelease? release = await FredClient.GetRelease(grp.Key);

                    if (release is null)
                        throw new Exception("release is null");
                }
            }
        }
    }
}
