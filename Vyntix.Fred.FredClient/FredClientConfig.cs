using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaderAnalytics.Vyntix.Fred.FredClient
{
    public class FredClientConfig
    {
        public static string BaseAPIURL = "https://api.stlouisfed.org/fred/";
        
        public string BaseURL { get; init; } = BaseAPIURL;
        public int MaxDownloadRetries { get; init; } = 4;
        public int MaxConcurrentDownloads { get; init;} = 4;
        public int ErrorDelay { get; init; } = 1000;                            // Number of milliseconds to wait before trying again if we get an error.
        public int MaxRequestsPerMinute { get; init; } = 120;                   // Maximum number of requests per minute their API will allow without a 429 error. 
    }
}
