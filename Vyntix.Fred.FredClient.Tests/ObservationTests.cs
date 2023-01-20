namespace LeaderAnalytics.Vyntix.Fred.FredClient.Tests;

[TestFixture(FredFileType.JSON)]
[TestFixture(FredFileType.XML)]
public class ObservationTests : BaseTest
{
    

    public ObservationTests(FredFileType fileType) : base(fileType)
    {

    }

    [Test]
    public async Task gnpca_gets_all_observations()
    {
        List<Observation> data = await FredClient.GetObservations("GNPCA");
        Assert.IsNotNull(data);
    }

    [Test]
    public async Task invalid_symbol_thows()
    {
        // We don't throw because the symbol is invalid - we throw because there are no vintage dates passed
        // to GetObservations.  
        Assert.ThrowsAsync<Exception>(() => FredClient.GetObservations(DOES_NOT_EXIST));
    }

    [Test]
    public async Task gnpca_get_vintages_for_realtime_dates()
    {
        List<Observation> data = await FredClient.GetObservations("GNPCA", new DateTime(2020, 1, 1), new DateTime(2020, 12, 31), DataDensity.Sparse);
        List<DateTime> vintageDates = data.GroupBy(x => x.VintageDate).Select(x => x.Key).ToList();
        Assert.IsNotNull(data);
        Assert.AreEqual(2, vintageDates.Count);
        Assert.AreEqual(5, data.Count);
    }

    [Test]
    public async Task GetObservationsTest3()
    {
        List<DateTime> vintateDates = new List<DateTime>(10)
            {
                new DateTime(2020, 1, 1),
                new DateTime(2020, 1, 1),
                new DateTime(2020, 1, 1),
                new DateTime(2020, 1, 1),
                new DateTime(2020, 1, 1),
                new DateTime(2020, 1, 1),
                new DateTime(2020, 1, 1),
                new DateTime(2020, 1, 1),
                new DateTime(2020, 1, 1),
                new DateTime(2020, 1, 1)
            };
        List<Observation> data = await FredClient.GetObservations("GNPCA", vintateDates, DataDensity.Sparse);
        Assert.IsNotNull(data);

       
    }



















    [Test]
    public async Task gdp_returns_sparse_data_for_selected_vintage_dates()
    {
        string symbol = "gdp";
        List<DateTime> vintagedates = new List<DateTime> { DateTime.Parse("2022-08-25"), DateTime.Parse("2022-09-29"), DateTime.Parse("2022-10-27"), DateTime.Parse("2022-11-30"), DateTime.Parse("2022-12-22") };
        List<Observation> observations = await FredClient.GetObservations(symbol, vintagedates, DataDensity.Sparse);
        Assert.AreEqual(23, observations.Count);
    }

    [Test]
    public async Task gdp_returns_dense_data_for_selected_vintage_dates()
    {
        string symbol = "gdp";
        List<DateTime> vintagedates = new List<DateTime> { DateTime.Parse("2022-08-25"), DateTime.Parse("2022 -09-29"), DateTime.Parse("2022 -10-27"), DateTime.Parse("2022 -11-30"), DateTime.Parse("2022 -12-22") };
        List<Observation> observations = await FredClient.GetObservations(symbol, vintagedates, DataDensity.Dense);
        Assert.AreEqual(303, observations.Count);
    }

    [Test]
    public async Task gdp_vintage_test_gets_all_vintages()
    {
        List<Vintage> vintages = await FredClient.GetVintages("gdp");
        Assert.IsNotNull(vintages); 
        Assert.Greater(vintages.Count, 10);
    }

    [Test]
    public async Task gdp_vintage_test_gets_exact_vintages_for_range()
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

