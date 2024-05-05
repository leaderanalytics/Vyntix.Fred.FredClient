namespace LeaderAnalytics.Vyntix.Fred.FredClient.Tests;

[TestFixture(FredFileType.JSON)]
[TestFixture(FredFileType.XML)]
public class AuthenticationTests : BaseTest
{
    public AuthenticationTests(FredFileType fileType) : base(fileType)
    {

    }

    [Test]
    public async Task Authentication_fails_when_api_key_is_invald()
    {
        bool isauthenticated = await FredClient.IsAPI_KeyValid();
        Assert.IsTrue(isauthenticated);
        BuildFredClient(DOES_NOT_EXIST);  // Build FredClient with an invalid API key
        isauthenticated = await FredClient.IsAPI_KeyValid();
        Assert.IsFalse(isauthenticated);
    }
}
