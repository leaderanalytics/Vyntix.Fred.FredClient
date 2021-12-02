// https://github.com/dotnet/runtime/issues/40452
// https://github.com/dotnet/runtime/issues/49598

namespace LeaderAnalytics.Vyntix.Fred.FredClient;

public class JsonFredClient : BaseFredClient
{

    public JsonFredClient(string apiKey, FredClientConfig config, IVintageComposer composer, HttpClient httpClient, ILogger<IFredClient> logger) : base(apiKey, config, composer, httpClient, logger)
    {

    }

    protected override async Task<T> Parse<T>(string uri, string root)
    {
        try
        {
            uri = uri + (uri.Contains("?") ? "&" : "?") + "file_type=json";

            using (Stream stream = await Download(uri))
            {

                if (stream is null)
                    return default(T);

                using (JsonDocument document = JsonDocument.Parse(stream))
                {
                    string json = document.RootElement.GetProperty(root).GetRawText();
                    return JsonSerializer.Deserialize<T>(json);
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"JSONFredClient encountered an error. URI is {uri}, type is {typeof(T).FullName}, root is {root}.  See the inner exception for more detail.", ex);
        }
    }


    public override async Task<List<Vintage>> GetVintageDates(string symbol, DateTime? RTStart)
    {
        string uri = "series/vintagedates?series_id=" + symbol;

        if (RTStart != null)
            uri += "&realtime_start=" + RTStart.Value.Date.ToString(FRED_DATE_FORMAT);

        int offset = -10000;
        bool doIt = true;
        List<Vintage> result = new List<Vintage>(1500);
        List<DateTime> vintages = null;

        while (doIt)
        {
            string newUri;
            offset += 10000;
            newUri = uri + "&offset=" + offset.ToString();
            vintages = (await Parse<List<DateTime>>(newUri, "vintage_dates"))?.ToList();

            if (vintages != null)
                result.AddRange(vintages.Select(x => new Vintage { VintageDate = x }));
            else
                break;

            doIt = vintages.Count == 10000;
        }
        result.ForEach(x => x.Symbol = symbol);
        return result.Any() ? result : null;
    }
}
