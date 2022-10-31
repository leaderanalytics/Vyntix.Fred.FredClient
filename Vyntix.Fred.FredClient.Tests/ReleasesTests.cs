namespace LeaderAnalytics.Vyntix.Fred.FredClient.Tests;

[TestFixture(FredFileType.JSON)]
[TestFixture(FredFileType.XML)]
public class ReleasesTests : BaseTest
{

    public ReleasesTests(FredFileType fileType) : base(fileType)
    {
    }

    [Test]
    public async Task GetAllReleasesTest()
    {
        List<Release> data = await FredClient.GetAllReleases();
        Assert.IsNotNull(data);
        Assert.IsTrue(data.Any());
    }

    [Test]
    public async Task GetAllReleaseDatesTest()
    {
        List<ReleaseDate> data = await FredClient.GetAllReleaseDates(null, true);
        Assert.IsNotNull(data);
        Assert.IsTrue(data.Any());

        data = await FredClient.GetAllReleaseDates(new DateTime(2022, 8, 1), true);
        Assert.IsNotNull(data);
        Assert.IsTrue(data.Any());
    }

    [Test]
    public async Task GetReleaseTest()
    {
        Release data = await FredClient.GetRelease("53");
        Assert.IsNotNull(data);
    }


    [Test]
    public async Task GetReleaseDatesForReleaseTest1()
    {
        ReleaseDate data = (await FredClient.GetReleaseDates("82", 0)).FirstOrDefault();
        Assert.IsNotNull(data);
        Assert.IsFalse(IsZeroString(data.ReleaseID));
        Assert.AreNotEqual(DateTime.MinValue, data.DateReleased);
    }

    [Test]
    public async Task GetReleaseDatesForReleaseTest2()
    {
        List<ReleaseDate>  data = await FredClient.GetReleaseDatesForRelease("82", DateTime.Now.AddMonths(-1), true);
        Assert.IsNotNull(data);
    }

    [Test]
    public async Task GetSeriesForReleaseTest()
    {
        List<Series> data = await FredClient.GetSeriesForRelease("51");
        Assert.IsNotNull(data);
    }

    [Test]
    public async Task GetReleasesForSourceTest()
    {
        Release data = (await FredClient.GetReleasesForSource("1")).FirstOrDefault();
        Assert.IsNotNull(data);
        Assert.IsFalse(IsZeroString(data.NativeID));
        Assert.IsFalse(String.IsNullOrEmpty(data.Name));
        Assert.AreNotEqual(DateTime.MinValue, data.RTStart);
    }

    [Test]
    public async Task GetReleaseForSeriesTest()
    {
        Release data = (await FredClient.GetReleaseForSeries("IRA"));
        Assert.IsNotNull(data);
        Assert.IsFalse(IsZeroString(data.NativeID));
        Assert.IsFalse(String.IsNullOrEmpty(data.Name));
    }

    [Test]
    public async Task GetSourcesForReleaseTest()
    {
        List<Source> data = await FredClient.GetSourcesForRelease("51");
        Assert.IsNotNull(data);
    }
}
