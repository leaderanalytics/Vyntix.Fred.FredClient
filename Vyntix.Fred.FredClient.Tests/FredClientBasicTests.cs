namespace LeaderAnalytics.Vyntix.Fred.FredClient.Tests;

[TestFixture(FredFileType.JSON)]
[TestFixture(FredFileType.XML)]
public class FredClientBasicTests : BaseTest
{
    

    public FredClientBasicTests(FredFileType fileType) : base(fileType)
    {

    }

    [Test]
    public async Task GetCategoriesForSeriesTest()
    {
        List<FredCategory> data = await FredClient.GetCategoriesForSeries("EXJPUS");
        Assert.That(data, Is.Not.Null);

        data = await FredClient.GetCategoriesForSeries(DOES_NOT_EXIST);
        Assert.That(data, Is.Null);
    }

    [Test]
    public async Task GetCategoryTest()
    {
        FredCategory data = await FredClient.GetCategory("125");
        Assert.That(data, Is.Not.Null);

        data = await FredClient.GetCategory(DOES_NOT_EXIST);
        Assert.That(data, Is.Null);
    }

    [Test]
    public async Task GetCategoryChildrenTest()
    {
        List<FredCategory> data = await FredClient.GetCategoryChildren("13");
        Assert.That(data, Is.Not.Null);

        data = await FredClient.GetCategoryChildren(DOES_NOT_EXIST);
        Assert.That(data, Is.Null);
    }

    [Test]
    public async Task GetCategoryTagsTest()
    {
        List<FredCategoryTag> data = await FredClient.GetCategoryTags("125");
        Assert.That(data, Is.Not.Null);

        data = await FredClient.GetCategoryTags(DOES_NOT_EXIST);
        Assert.That(data, Is.Null);
    }

  
    [Test]
    public async Task GetRelatedCategoriesTest()
    {
        List<FredRelatedCategory> data = await FredClient.GetRelatedCategories("32073");
        Assert.That(data, Is.Not.Null);

        data = await FredClient.GetRelatedCategories(DOES_NOT_EXIST);
        Assert.That(data, Is.Null);
    }


    [Test]
    public async Task GetReleaseDatesTest()
    {
        List<FredReleaseDate> data = await FredClient.GetReleaseDates("82", 0);
        Assert.That(data, Is.Not.Null);

        data = await FredClient.GetReleaseDates(DOES_NOT_EXIST, 0);
        Assert.That(data, Is.Null);
    }


    [Test]
    public async Task GetReleasesForSourceTest()
    {
        List<FredRelease> data = await FredClient.GetReleasesForSource("1");
        Assert.That(data, Is.Not.Null);

        data = await FredClient.GetReleasesForSource(DOES_NOT_EXIST);
        Assert.That(data, Is.Null);
    }

    [Test]
    public async Task GetReleasesForSourceTest2()
    {
        List<FredRelease> data = await FredClient.GetReleasesForSource("1", new DateTime(2020, 1, 1), new DateTime(2020, 12, 31));
        Assert.That(data, Is.Not.Null);

        data = await FredClient.GetReleasesForSource(DOES_NOT_EXIST, new DateTime(2020, 1, 1), new DateTime(2020, 12, 31));
        Assert.That(data, Is.Null);
    }


    [Test]
    public async Task GetSeriesTest()
    {
        FredSeries data = await FredClient.GetSeries("GNPCA");
        Assert.That(data, Is.Not.Null);

        data = await FredClient.GetSeries(DOES_NOT_EXIST);
        Assert.That(data, Is.Null);
    }

    [Test]
    public async Task GetSeriesForCategoryTest()
    {
        List<FredSeries> data = await FredClient.GetSeriesForCategory("125", false);
        Assert.That(data, Is.Not.Null);

        data = await FredClient.GetSeriesForCategory(DOES_NOT_EXIST, false);
        Assert.That(data, Is.Null);
    }

    [Test]
    public async Task GetSeriesForReleaseTest()
    {
        List<FredSeries> data = await FredClient.GetSeriesForRelease("51");
        Assert.That(data, Is.Not.Null);

        data = await FredClient.GetSeriesForRelease(DOES_NOT_EXIST);
        Assert.That(data.Any(), Is.False);
    }

    [Test]
    public async Task GetSeriesTagsTest()
    {
        List<FredSeriesTag> data = await FredClient.GetSeriesTags("STLFSI");
        Assert.That(data, Is.Not.Null);

        data = await FredClient.GetSeriesTags(DOES_NOT_EXIST);
        Assert.That(data, Is.Null);
    }

    [Test]
    public async Task GetSourcesTest()
    {
        List<FredSource> data = await FredClient.GetSources();
        Assert.That(data, Is.Not.Null);
    }

    [Test]
    public async Task GetSourcesTest1()
    {
        List<FredSource> data = await FredClient.GetSources(new DateTime(2020, 1, 1), new DateTime(2020, 12, 31));
        Assert.That(data, Is.Not.Null);
    }

    [Test]
    public async Task GetVintgeDatesTest()
    {
        List<FredVintage> data = (await FredClient.GetVintages("GNPCA", null)).Data;
        Assert.That(data, Is.Not.Null);

        data = (await FredClient.GetVintages("GNPCA", new DateTime(1959, 2, 19), null)).Data;
        Assert.That(new DateTime(1959, 2, 19), Is.EqualTo( data.First().VintageDate));

        data = (await FredClient.GetVintages("GNPCA", null, new DateTime(1961, 7, 19))).Data;
        Assert.That(new DateTime(1961, 7, 19), Is.EqualTo(data.Last().VintageDate));


        data = (await FredClient.GetVintages("GNPCA", new DateTime(1959, 2, 19), new DateTime(1959, 7, 19))).Data;
        Assert.That(2, Is.EqualTo(data.Count));

        data = (await FredClient.GetVintages(DOES_NOT_EXIST, null)).Data;
        Assert.That(data, Is.Not.Null);
    }
}
