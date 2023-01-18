using LeaderAnalytics.Vyntix.Fred.Domain;
using LeaderAnalytics.Vyntix.Fred.Model;

namespace LeaderAnalytics.Vyntix.Fred.FredClient.Tests;

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
            tasks[i] = FredClient.GetObservations(symbols[i], startDate, endDate, DataDensity.Dense)
                .ContinueWith(x => x.Result.ForEach(o => observations.Add(o)));

        Task result = Task.WhenAll(tasks);
        await result;
        Assert.IsFalse(result.IsFaulted);
        Assert.AreEqual(5, observations.GroupBy(x => x.Symbol).Count());
    }


    [Test()]
    public async Task EnduranceTest()
    {
        for (int i = 0; i < 10; i++)
        {
            List<ReleaseDate> dates = await FredClient.GetAllReleaseDates(null, true);

            if (dates is null)
                throw new Exception("dates is null");


            if (dates?.Any() ?? false)
            {
                foreach (var grp in dates.GroupBy(x => x.ReleaseID))
                {
                    Release? release = await FredClient.GetRelease(grp.Key);

                    if (release is null)
                        throw new Exception("release is null");
                }
            }
        }
    }
}
