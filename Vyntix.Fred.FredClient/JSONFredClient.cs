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
            
            if(json is null)
                return default(T);
            
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            throw new Exception($"JSONFredClient encountered an error while deserializing objects of type {typeof(T).FullName}. URI is {uri},  root is {root}.  See the inner exception for more detail.", ex);
        }
    }

    protected override async Task<List<FredObservation>> ParseObservations(string symbol, string uri)
    {
        // Raw data:  {"date":"2022-12-01","BAA10Y_20221202":"2.3","BAA10Y_20221206":"2.09"},

        List<FredObservation> observations = new(2000);
        string json = await GetJson(uri, "observations");

        if (json is null)
            return null;
        
        try
        {
            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                // Traverse rows
                foreach (JsonElement obs in doc.RootElement.EnumerateArray())
                {
                    JsonProperty[] properties = obs.EnumerateObject().ToArray();

                    // Traverse columns.  Missing columns are common.
                    for (int i = 1; i < properties.Length; i++)
                    {
                        string stringVal = properties[i].Value.GetString();

                        if (!string.IsNullOrEmpty(stringVal) && stringVal != ".")
                        {
                            observations.Add(new FredObservation
                            {
                                Symbol = symbol,
                                ObsDate = properties[0].Value.GetDateTime(),
                                VintageDate = DateTime.ParseExact(properties[i].Name.Split("_")[1], "yyyyMMdd", CultureInfo.InvariantCulture),
                                Value = stringVal
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"JSONFredClient encountered an error in ParseObservations. URI is {uri}, symbol is {symbol}, json is {json}.  See the inner exception for more detail.", ex);
        }
        return observations;
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

    protected override async Task<List<DateTime>> ParseVintageDates(string uri, string root) => await Parse<List<DateTime>>(uri, root);
}
    
        
    
