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

    protected override async Task<List<Observation>> ParseObservations(string symbol, string uri)
    {
        List<Observation> observations = new(2000);

        try
        {
            using (Stream stream = await Download(uri))
            {
                XmlDocument doc = new();
                doc.Load(stream);
             
                foreach(XmlNode node in doc.DocumentElement.ChildNodes)
                {
                    string stringVal = node.Attributes[1].Value;

                    if (!string.IsNullOrEmpty(stringVal) && stringVal != ".")
                    {
                        observations.Add(new Observation
                        {
                            Symbol = symbol,
                            ObsDate = DateTime.ParseExact(node.Attributes[0].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                            VintageDate = DateTime.ParseExact(node.Attributes[1].Name.Split("_")[1], "yyyyMMdd", CultureInfo.InvariantCulture),
                            Value = stringVal

                        });
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
}
