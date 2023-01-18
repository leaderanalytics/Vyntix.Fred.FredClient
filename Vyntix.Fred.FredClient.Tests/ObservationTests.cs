namespace LeaderAnalytics.Vyntix.Fred.FredClient.Tests;

[TestFixture(FredFileType.JSON)]
[TestFixture(FredFileType.XML)]
public class ObservationTests : BaseTest
{
    

    public ObservationTests(FredFileType fileType) : base(fileType)
    {

    }

    [Test]
    public async Task gdp_tests()
    {
        string symbol = "gdp";
        List<DateTime> vintagedates = new List<DateTime> { DateTime.Parse("2022-08-25"), DateTime.Parse("2022 -09-29"), DateTime.Parse("2022 -10-27"), DateTime.Parse("2022 -11-30"), DateTime.Parse("2022 -12-22") };
        List<Observation> observations = await FredClient.GetObservations(symbol, vintagedates, DataDensity.Sparse);
    }

    [Test]
    public async Task gdp_vintage_test_gets_all_vintages()
    {
        List<Vintage> vintages = await FredClient.GetVintages("gdp");
        Assert.IsNotNull(vintages); 
        Assert.Greater(vintages.Count, 10);
    }

    [Test]
    public async Task gdp_vintage_test_gets_exact_vintages()
    {
        // These four dates are valid vintage dates for gdp:
        // 1992-01-29
        // 1992-02-28
        // 1992-03-26
        // 1992-04-28

        List<Vintage> vintages = await FredClient.GetVintages("gdp", DateTime.Parse("1992-01-29"), DateTime.Parse("1992-04-28"));
        Assert.IsNotNull(vintages);
        Assert.AreEqual(vintages.Count, 4);
    }
}

