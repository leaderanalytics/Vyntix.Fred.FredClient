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
        Assert.IsNotNull(data);
        Assert.IsFalse(IsZeroString(data.NativeID));
        Assert.IsFalse(String.IsNullOrEmpty(data.Name));
        Assert.IsFalse(IsZeroString(data.ParentID));
    }

    [Test]
    public async Task ObservationTest()
    {
        FredObservation data = (await FredClient.GetObservations("GNPCA")).Data.FirstOrDefault();
        Assert.IsNotNull(data);
        Assert.IsFalse(String.IsNullOrEmpty(data.Symbol));
        Assert.IsFalse(String.IsNullOrEmpty(data.Value));
        Assert.AreNotEqual(DateTime.MinValue, data.ObsDate);
        Assert.AreNotEqual(DateTime.MinValue, data.VintageDate);
    }

    [Test]
    public async Task RelatedCategoryTest()
    {
        FredRelatedCategory data = (await FredClient.GetRelatedCategories("32073")).FirstOrDefault();
        Assert.IsNotNull(data);
        Assert.IsFalse(IsZeroString(data.CategoryID));
        Assert.IsFalse(IsZeroString(data.RelatedCategoryID));
    }

    

    [Test]
    public async Task SeriesTest()
    {
        FredSeries data = (await FredClient.GetSeries("GNPCA"));
        Assert.IsNotNull(data);
        Assert.IsFalse(String.IsNullOrEmpty(data.Symbol));
        Assert.IsFalse(String.IsNullOrEmpty(data.Title));
        Assert.IsFalse(String.IsNullOrEmpty(data.Frequency));
        Assert.IsFalse(String.IsNullOrEmpty(data.Units));
        Assert.IsFalse(String.IsNullOrEmpty(data.SeasonalAdj));
    }

    [Test]
    public async Task SeriesCategoryTest()
    {
        FredSeries data = (await FredClient.GetSeriesForCategory("125", false)).FirstOrDefault();
        Assert.IsNotNull(data);
        Assert.IsFalse(String.IsNullOrEmpty(data.Symbol));
    }

    [Test]
    public async Task SeriesTagTest()
    {
        FredSeriesTag data = (await FredClient.GetSeriesTags("STLFSI")).FirstOrDefault();
        Assert.IsNotNull(data);
        Assert.IsFalse(String.IsNullOrEmpty(data.Symbol));
        Assert.IsFalse(String.IsNullOrEmpty(data.Name));
        Assert.IsFalse(String.IsNullOrEmpty(data.GroupID));
        Assert.IsFalse(String.IsNullOrEmpty(data.Notes));
        Assert.Greater(data.Popularity, 0);
    }

    [Test]
    public async Task SourceTest()
    {
        FredSource data = (await FredClient.GetSources()).FirstOrDefault();
        Assert.IsNotNull(data);
        Assert.IsFalse(String.IsNullOrEmpty(data.NativeID));
        Assert.IsFalse(String.IsNullOrEmpty(data.Name));
        Assert.IsFalse(String.IsNullOrEmpty(data.Link));
    }

    [Test]
    public async Task VintageTest()
    {
        FredVintage data = (await FredClient.GetVintages("GNPCA", null)).Data.FirstOrDefault();
        Assert.IsNotNull(data);
        Assert.IsFalse(String.IsNullOrEmpty(data.Symbol));
        Assert.AreNotEqual(DateTime.MinValue, data.VintageDate);
    }
}
