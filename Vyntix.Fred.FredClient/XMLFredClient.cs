using System.Globalization;
using System.Xml;

namespace LeaderAnalytics.Vyntix.Fred.FredClient;

public class XMLFredClient : BaseFredClient
{
    public XMLFredClient(string apiKey, FredClientConfig config, IVintageComposer composer, HttpClient httpClient, ILogger<IFredClient> logger) : base(apiKey, config, composer, httpClient, logger) { }

    protected override async Task<T> Parse<T>(string uri, string root)
    {
        try
        {
            using (Stream stream = await Download(uri))
                return stream == null ? default(T) : SerializationHelper<T>.DeSerialize(stream, root);
        }
        catch (Exception ex)
        {
            throw new Exception($"XMLFredClient encountered an error. URI is {uri}, type is {typeof(T).FullName}, root is {root}.  See the inner exception for more detail.", ex);
        }
    }

    protected override async Task<List<FredObservation>> ParseObservations(string symbol, string uri)
    {
        List<FredObservation> observations = new(2000);

        try
        {
            using (Stream stream = await Download(uri))
            {
                if (stream is null)
                    return null;

                XmlDocument doc = new();
                doc.Load(stream);
             
                // Traverse rows
                foreach(XmlNode node in doc.DocumentElement.ChildNodes)
                {
                    // Traverse columns.  Missing columns are common: CPIAUCSL <observation date="1992-07-01"/> has no observations.
                    for (int i = 1; i < node.Attributes.Count; i++)
                    {
                        string stringVal = node.Attributes[i].Value;

                        if (!string.IsNullOrEmpty(stringVal) && stringVal != ".")
                        {
                            observations.Add(new FredObservation
                            {
                                Symbol = symbol,
                                ObsDate = DateTime.ParseExact(node.Attributes[0].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                                VintageDate = DateTime.ParseExact(node.Attributes[i].Name.Split("_")[1], "yyyyMMdd", CultureInfo.InvariantCulture),
                                Value = stringVal

                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"XMLFredClient encountered an error parsing Observations. URI is {uri}.  See the inner exception for more detail.", ex);
        }
        return observations;
    }

    protected override async Task<List<DateTime>> ParseVintageDates(string uri, string root)
    {
        List<DateTime> dates = new(150);

        try
        {
            using (Stream stream = await Download(uri))
            {
                if (stream is null)
                    return null;

                XmlDocument doc = new();
                doc.Load(stream);

                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                    dates.Add(DateTime.ParseExact(node.InnerText, "yyyy-MM-dd", CultureInfo.InvariantCulture));
                
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"XMLFredClient encountered an error parsing Vintage Dates. URI is {uri}.  See the inner exception for more detail.", ex);
        }
        return dates;
    }
}
