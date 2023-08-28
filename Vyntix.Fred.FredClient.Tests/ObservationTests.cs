using static System.Runtime.InteropServices.JavaScript.JSType;

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
        List<FredObservation> data = await FredClient.GetObservations("GNPCA");
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
    public async Task gnpca_gets_vintages_for_realtime_dates()
    {
        DateTime realTimeStart = new DateTime(2019, 5, 1);
        DateTime realTimeEnd = new DateTime(2020, 5, 1);
        DateTime obsPeriod = new DateTime(2018, 1, 1);

        List<FredObservation> data = await FredClient.GetObservations("GNPCA", obsPeriod, realTimeStart, realTimeEnd, DataDensity.Sparse);
        List<DateTime> vintageDates = data.GroupBy(x => x.VintageDate).Select(x => x.Key).ToList();
        Assert.IsNotNull(data);
        Assert.AreEqual(2, vintageDates.Count);
        Assert.AreEqual(2, data.Count);
        Assert.AreEqual(new DateTime(2019,03,28).Date, data[0].VintageDate.Date);
        Assert.AreEqual(new DateTime(2019, 07, 26).Date, data[1].VintageDate.Date);
    }

    [Test]
    public async Task get_vintage_dates_returns_inital_vintage_when_inital_vintage_is_multiple_vintages_in_the_past_sparse()
    {
        /*
         * Real-time start: 2017-10-05
         * Real-time end: 2018-08-20
         * Observation date: 2012-01-01
         * Vintage dates
         * 2015-07-30  15562.1
         * 2016-03-25  -- OMMITTED FROM SPARSE RESULT --
         * 2016-07-29  -- OMMITTED FROM SPARSE RESULT --
         * 2017-03-30  -- OMMITTED FROM SPARSE RESULT --
         * 2017-07-28  -- OMMITTED FROM SPARSE RESULT --
         * 2017-10-27  15562.122
         * 2018-03-28  -- OMMITTED FROM SPARSE RESULT --
         * 2018-07-27  16429.308
         */

        DateTime realTimeStart = new DateTime(2017, 10, 5);
        DateTime realTimeEnd = new DateTime(2018, 8, 20);
        DateTime obsPeriod = new DateTime(2012, 1, 1);
        
        List<FredObservation> data = await FredClient.GetObservations("GNPCA", obsPeriod, realTimeStart, realTimeEnd, DataDensity.Sparse);
        List<DateTime> vintageDates = data.GroupBy(x => x.VintageDate).Select(x => x.Key).ToList();
        Assert.IsNotNull(data);
        Assert.AreEqual(3, vintageDates.Count);
        Assert.AreEqual(3, data.Count);
        Assert.AreEqual(new DateTime(2015, 07, 30).Date, data[0].VintageDate.Date);
        Assert.AreEqual("15562.1", data[0].Value);
        Assert.AreEqual(new DateTime(2017, 10, 27).Date, data[1].VintageDate.Date);
        Assert.AreEqual("15562.122", data[1].Value);
        Assert.AreEqual(new DateTime(2018, 07, 27).Date, data[2].VintageDate.Date);
        Assert.AreEqual("16429.308", data[2].Value);
    }

    [Test]
    public async Task get_vintage_dates_returns_inital_vintage_when_inital_vintage_is_multiple_vintages_in_the_past_dense()
    {
        /*
         * Real-time start: 2017-10-05
         * Real-time end: 2018-08-20
         * Observation date: 2012-01-01
         * Vintage dates
         * 2015-07-30  15562.1
         * 2016-03-25  15562.1
         * 2016-07-29  15562.1
         * 2017-03-30  15562.1
         * 2017-07-28  15562.1
         * 2017-10-27  15562.122
         * 2018-03-28  15562.122
         * 2018-07-27  16429.308
         */

        DateTime realTimeStart = new DateTime(2017, 10, 5);
        DateTime realTimeEnd = new DateTime(2018, 8, 20);
        DateTime obsPeriod = new DateTime(2012, 1, 1);

        List<FredObservation> data = await FredClient.GetObservations("GNPCA", obsPeriod, realTimeStart, realTimeEnd, DataDensity.Dense);
        List<DateTime> vintageDates = data.GroupBy(x => x.VintageDate).Select(x => x.Key).ToList();
        Assert.IsNotNull(data);
        Assert.AreEqual(8, vintageDates.Count);
        Assert.AreEqual(8, data.Count);
        Assert.AreEqual(new DateTime(2015, 07, 30).Date, data[0].VintageDate.Date);
        Assert.AreEqual(new DateTime(2016, 03, 25).Date, data[1].VintageDate.Date);
        Assert.AreEqual(new DateTime(2016, 07, 29).Date, data[2].VintageDate.Date);
        Assert.AreEqual(new DateTime(2017, 03, 30).Date, data[3].VintageDate.Date);
        Assert.AreEqual(new DateTime(2017, 07, 28).Date, data[4].VintageDate.Date);
        Assert.AreEqual(new DateTime(2017, 10, 27).Date, data[5].VintageDate.Date);
        Assert.AreEqual(new DateTime(2018, 03, 28).Date, data[6].VintageDate.Date);
        Assert.AreEqual(new DateTime(2018, 07, 27).Date, data[7].VintageDate.Date);

    }


    



    [Test]
    public async Task gdp_returns_sparse_data_for_selected_vintage_dates()
    {
        string symbol = "gdp";
        List<DateTime> vintagedates = new List<DateTime> { DateTime.Parse("2022-08-25"), DateTime.Parse("2022-09-29"), DateTime.Parse("2022-10-27"), DateTime.Parse("2022-11-30"), DateTime.Parse("2022-12-22") };
        List<FredObservation> observations = await FredClient.GetObservations(symbol, vintagedates, DataDensity.Sparse);
        Assert.AreEqual(26, observations.Count);
    }

    [Test]
    public async Task gdp_returns_dense_data_for_selected_vintage_dates()
    {
        string symbol = "gdp";
        List<DateTime> vintagedates = new List<DateTime> { DateTime.Parse("2022-08-25"), DateTime.Parse("2022 -09-29"), DateTime.Parse("2022 -10-27"), DateTime.Parse("2022 -11-30"), DateTime.Parse("2022 -12-22") };
        List<FredObservation> observations = await FredClient.GetObservations(symbol, vintagedates, DataDensity.Dense);
        Assert.AreEqual(1513, observations.Count);
    }

    [Test]
    public async Task gdp_vintage_test_gets_all_vintages()
    {
        List<FredVintage> vintages = await FredClient.GetVintages("gdp");
        Assert.IsNotNull(vintages); 
        Assert.Greater(vintages.Count, 10);
    }

    [Test]
    public async Task gdp_vintage_test_gets_exact_vintages_for_range()
    {
        // These four dates are valid vintage dates for gdp:
        // 1991-12-04
        // 1991-12-20
        // 1992-01-29
        // 1992-02-28
        // 1992-03-26
        // 1992-04-28

        List<FredVintage> vintages = await FredClient.GetVintages("gdp", DateTime.Parse("1991-12-04"),  DateTime.Parse("1992-04-28"));
        Assert.IsNotNull(vintages);
        Assert.AreEqual(vintages.Count, 6);
    }

    [Test]
    public async Task no_observatons_returned_when_no_vintages_exist_within_realtime_period()
    {
        // The first vintage for gdp is 1991-12-04.  No vintages existed 
        // within the realtime period specified.
        DateTime realTimeStart = new DateTime(1970, 1, 1);
        DateTime realTimeEnd = new DateTime(1979, 12, 31);
        DateTime observationPeriodStart = new DateTime(1975, 4, 1);  
        List<FredObservation> observations = await FredClient.GetObservations("gdp", observationPeriodStart, realTimeStart, realTimeEnd, DataDensity.Dense);
        Assert.AreEqual(observations.Count, 0);
    }


    [Test]
    public async Task earliest_vintage_before_realtime_end_is_returned_when_no_vintage_before_realtime_start_exists()
    {
        // The first vintage for gdp is 1991-12-04 which is after the real time start.
        DateTime realTimeStart = new DateTime(1990, 1, 1);
        DateTime realTimeEnd = new DateTime(1999, 12, 31);
        DateTime observationPeriodStart = new DateTime(1975, 4, 1);
        List<FredObservation> observations = await FredClient.GetObservations("gdp", observationPeriodStart, realTimeStart, realTimeEnd, DataDensity.Dense);
        Assert.GreaterOrEqual(observations.Count, 97);
        Assert.AreEqual(observations.First().VintageDate, new DateTime(1991, 12, 4));
    }
}

