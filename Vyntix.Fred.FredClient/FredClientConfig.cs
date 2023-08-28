namespace LeaderAnalytics.Vyntix.Fred.FredClient;

public class FredClientConfig
{
    public static string BaseAPIURL = "https://api.stlouisfed.org/fred/";

    public string BaseURL { get; init; } = BaseAPIURL;
    public int MaxDownloadRetries { get; init; } = 4;
    public int MaxConcurrentDownloads { get; init; } = 2;
    public int ErrorDelay { get; init; } = 2000;                            // Number of milliseconds to wait before trying again if we get an error.
    public int MaxRequestsPerMinute { get; init; } = 100;                   // Maximum number of requests per minute their API will allow without a 429 error. 
    public int VintageChunkSize { get; init; } = 500;
}
