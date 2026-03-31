

namespace LeaderAnalytics.Vyntix.Fred.FredClient.Tests;

[TestFixture(FredFileType.JSON)]
[TestFixture(FredFileType.XML)]
public class NoVintageSeriesTests : BaseTest
{
    public NoVintageSeriesTests(FredFileType fileType) : base(fileType)
    {

    }

    [Test]
    public async Task GetNonVintagesTest()
    {
        List<FredObservation> obs = await FredClient.GetNonVintageObservations("SP500");
        Assert.That(obs, Is.Not.Null);
        Assert.That(obs.All(x => x.VintageDate == x.ObsDate), Is.True);
    }

    [Test]
    public async Task GetNonVintages_for_invalid_symbol_fails()
    {
        List<FredObservation> obs = await FredClient.GetNonVintageObservations(DOES_NOT_EXIST);
        Assert.That(obs, Is.Null);
    }

    [Test]
    public async Task GetVintagesForNonVintageSeriesFails()
    {
        // Attempt to get vintage dates for a non-vintaged series
        // which should result in Success property of APIResult
        // being set to false.
        APIResult<List<FredVintage>> vintageResult = await FredClient.GetVintages("SP500");
        Assert.That(vintageResult.Success, Is.False);
        Assert.That(vintageResult.Message, Is.Not.Null);
    }
    [Test]
    public async Task GetObservationsForNonVintageSeriesFails()
    {
        // Attempt to get observations for a non-vintaged series
        // which should result in Success property of APIResult
        // being set to false.
        APIResult<List<FredObservation>> obsResult = await FredClient.GetObservations("SP500");
        Assert.That(obsResult.Success, Is.False);
        Assert.That(obsResult.Message, Is.Not.Null);
    }
}
