// https://github.com/dotnet/runtime/issues/40452
// https://github.com/dotnet/runtime/issues/49598

using System.Globalization;
using System.Text.Json.Nodes;

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
            string json = await GetJson(uri, root);
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex) 
        {
            throw new Exception($"JSONFredClient encountered an error while deserializing objects of type {typeof(T).FullName}. URI is {uri},  root is {root}.  See the inner exception for more detail.", ex);
        }
    }

    protected override async Task<List<Observation>> ParseObservations(string symbol, string uri)
    {
        // Raw data:  { "date":"2017-01-01","GDP_20220929":"19148.194"},

        List<Observation> observations = new(2000);
        string json = await GetJson(uri, "observations");
        
        using (JsonDocument doc = JsonDocument.Parse(json))
        {
            foreach(JsonElement obs in doc.RootElement.EnumerateArray()) 
            {
                JsonProperty[] properties = obs.EnumerateObject().ToArray();
                string stringVal = properties[1].Value.GetString();
                
                if (!string.IsNullOrEmpty(stringVal) && stringVal != ".")
                {
                    observations.Add(new Observation
                    {
                        Symbol = symbol,
                        ObsDate = properties[0].Value.GetDateTime(),
                        VintageDate = DateTime.ParseExact(properties[1].Name.Split("_")[1] ,"yyyyMMdd", CultureInfo.InvariantCulture),
                        Value = stringVal
                    });
                }
            }
        }
        return observations;
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

    private async Task<string> GetJson(string uri, string root)
    {
        try
        {
            uri = uri + (uri.Contains("?") ? "&" : "?") + "file_type=json";

            using (Stream stream = await Download(uri))
            {

                if (stream is null)
                    return null;

                using (JsonDocument document = JsonDocument.Parse(stream))
                {
                    return document.RootElement.GetProperty(root).GetRawText();
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"JSONFredClient encountered an error while downloading. URI is {uri}, root is {root}.  See the inner exception for more detail.", ex);
        }
    }
}
