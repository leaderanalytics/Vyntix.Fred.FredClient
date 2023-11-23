namespace Vyntix.Fred.FredClient.Tests;

public abstract class BaseTest
{
    private readonly string apiKey;
    protected const string BaseURL = "https://api.stlouisfed.org/fred/";
    protected IFredClient FredClient;
    protected readonly FredFileType CurrentFileType;
    protected bool IsZeroString(string s) => String.IsNullOrEmpty(s) || s == "0";
    protected const string DOES_NOT_EXIST = "DOES_NOT_EXIST"; // Arbitrary invalid identifier

    public BaseTest(FredFileType fileType)
    {
        CurrentFileType = fileType;
        string path = "O:\\LeaderAnalytics\\Config\\Vyntix.Fred.FredClient\\apiKey.txt";
        apiKey = System.IO.File.ReadAllText(path);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Debug()
            .CreateLogger();

        Log.Information("Logging has been configured.");
        BuildFredClient();
    }

    [SetUp]
    public void Setup() 
    {
    }

    
    private void BuildFredClient()
    {
        HttpClient httpClient = new HttpClient() { BaseAddress = new Uri(FredClientConfig.BaseAPIURL) };
        FredClientConfig config = new FredClientConfig { MaxDownloadRetries = 3, ErrorDelay = 2000, MaxRequestsPerMinute = 60 }; // MaxDownloadRetries should be greater than 1
        ILoggerFactory loggerFactory = new LoggerFactory().AddSerilog();
        ILogger<IFredClient> logger = loggerFactory.CreateLogger<IFredClient>();

        if (CurrentFileType == FredFileType.XML)
            FredClient = new XMLFredClient(apiKey, config, new VintageComposer(), httpClient, logger);
        else
            FredClient = new JsonFredClient(apiKey, config, new VintageComposer(), httpClient, logger);
    }
}
