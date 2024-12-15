namespace LeaderAnalytics.Vyntix.Fred.FredClient.Tests;

[TestFixture(FredFileType.JSON)]
[TestFixture(FredFileType.XML)]
public class ModelTests : BaseTest
{
    public ModelTests(FredFileType fileType) : base(fileType)
    {
    }

    

    [Test]
    public async Task CategoryTest()
    {
        FredCategory data = await FredClient.GetCategory("125");
        Assert.That(data, Is.Not.Null);
        Assert.That(IsZeroString(data.NativeID), Is.False);
        Assert.That(String.IsNullOrEmpty(data.Name), Is.False);
        Assert.That(IsZeroString(data.ParentID), Is.False);
    }

    [Test]
    public async Task ObservationTest()
    {
        FredObservation data = (await FredClient.GetObservations("GNPCA")).Data.FirstOrDefault();
        Assert.That(data, Is.Not.Null);
        Assert.That(String.IsNullOrEmpty(data.Symbol), Is.False);
        Assert.That(data.Value.HasValue, Is.True);
        Assert.That(DateTime.MinValue, Is.Not.EqualTo(data.ObsDate));
        Assert.That(DateTime.MinValue, Is.Not.EqualTo(data.VintageDate));
    }

    [Test]
    public async Task RelatedCategoryTest()
    {
        FredRelatedCategory data = (await FredClient.GetRelatedCategories("32073")).FirstOrDefault();
        Assert.That(data, Is.Not.Null);
        Assert.That(IsZeroString(data.CategoryID), Is.False);
        Assert.That(IsZeroString(data.RelatedCategoryID), Is.False);
    }

    

    [Test]
    public async Task SeriesTest()
    {
        FredSeries data = (await FredClient.GetSeries("GNPCA"));
        Assert.That(data, Is.Not.Null);
        Assert.That(String.IsNullOrEmpty(data.Symbol), Is.False);
        Assert.That(String.IsNullOrEmpty(data.Title), Is.False);
        Assert.That(String.IsNullOrEmpty(data.Frequency), Is.False);
        Assert.That(String.IsNullOrEmpty(data.Units), Is.False);
        Assert.That(String.IsNullOrEmpty(data.SeasonalAdj), Is.False);
    }

    [Test]
    public async Task SeriesCategoryTest()
    {
        FredSeries data = (await FredClient.GetSeriesForCategory("125", false)).FirstOrDefault();
        Assert.That(data, Is.Not.Null);
        Assert.That(String.IsNullOrEmpty(data.Symbol), Is.False);
    }

    [Test]
    public async Task SeriesTagTest()
    {
        FredSeriesTag data = (await FredClient.GetSeriesTags("STLFSI")).FirstOrDefault();
        Assert.That(data, Is.Not.Null);
        Assert.That(String.IsNullOrEmpty(data.Symbol), Is.False);
        Assert.That(String.IsNullOrEmpty(data.Name), Is.False);
        Assert.That(String.IsNullOrEmpty(data.GroupID), Is.False);
        Assert.That(String.IsNullOrEmpty(data.Notes), Is.False);
        Assert.That(data.Popularity, Is.GreaterThan(0));
    }

    [Test]
    public async Task SourceTest()
    {
        FredSource data = (await FredClient.GetSources()).FirstOrDefault();
        Assert.That(data, Is.Not.Null);
        Assert.That(String.IsNullOrEmpty(data.NativeID), Is.False);
        Assert.That(String.IsNullOrEmpty(data.Name), Is.False);
        Assert.That(String.IsNullOrEmpty(data.Link), Is.False);
    }

    [Test]
    public async Task VintageTest()
    {
        FredVintage data = (await FredClient.GetVintages("GNPCA", null)).Data.FirstOrDefault();
        Assert.That(data, Is.Not.Null);
        Assert.That(String.IsNullOrEmpty(data.Symbol), Is.False);
        Assert.That(DateTime.MinValue, Is.Not.EqualTo(data.VintageDate));
    }
}
