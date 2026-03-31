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
        List<FredRelease> data = await FredClient.GetAllReleases();
        Assert.That(data, Is.Not.Null);
        Assert.That(data.Any(), Is.True);
    }

    [Test]
    public async Task GetAllReleaseDatesTest()
    {
        List<FredReleaseDate> data = await FredClient.GetAllReleaseDates(null, true);
        Assert.That(data, Is.Not.Null);
        Assert.That(data.Any(), Is.True);

        data = await FredClient.GetAllReleaseDates(new DateTime(2022, 8, 1), true);
        Assert.That(data, Is.Not.Null);
        Assert.That(data.Any(), Is.True);
    }

    [Test]
    public async Task GetReleaseTest()
    {
        FredRelease data = await FredClient.GetRelease("53");
        Assert.That(data, Is.Not.Null);
    }


    [Test]
    public async Task GetReleaseDatesForReleaseTest1()
    {
        FredReleaseDate data = (await FredClient.GetReleaseDates("82", 0)).FirstOrDefault();
        Assert.That(data, Is.Not.Null);
        Assert.That(IsZeroString(data.ReleaseID), Is.False);
        Assert.That(DateTime.MinValue, Is.Not.EqualTo(data.DateReleased));
    }

    [Test]
    public async Task GetReleaseDatesForReleaseTest2()
    {
        List<FredReleaseDate>  data = await FredClient.GetReleaseDatesForRelease("82", DateTime.Now.AddMonths(-1), true);
        Assert.That(data, Is.Not.Null);
    }

    [Test]
    public async Task GetSeriesForReleaseTest()
    {
        List<FredSeries> data = await FredClient.GetSeriesForRelease("51");
        Assert.That(data, Is.Not.Null);
    }

    [Test]
    public async Task GetReleasesForSourceTest()
    {
        FredRelease data = (await FredClient.GetReleasesForSource("1")).FirstOrDefault();
        Assert.That(data, Is.Not.Null);
        Assert.That(IsZeroString(data.NativeID), Is.False);
        Assert.That(String.IsNullOrEmpty(data.Name), Is.False);
        Assert.That(DateTime.MinValue, Is.Not.EqualTo(data.RTStart));
    }

    [Test]
    public async Task GetReleaseForSeriesTest()
    {
        FredRelease data = (await FredClient.GetReleaseForSeries("IRA"));
        Assert.That(data, Is.Not.Null);
        Assert.That(IsZeroString(data.NativeID), Is.False);
        Assert.That(String.IsNullOrEmpty(data.Name), Is.False);
    }

    [Test]
    public async Task GetSourcesForReleaseTest()
    {
        List<FredSource> data = await FredClient.GetSourcesForRelease("51");
        Assert.That(data, Is.Not.Null);
    }
}
