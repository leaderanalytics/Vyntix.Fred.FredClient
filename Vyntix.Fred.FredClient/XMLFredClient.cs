using LeaderAnalytics.Vyntix.Fred.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LeaderAnalytics.Vyntix.Fred.FredClient
{
    public class XMLFredClient : BaseFredClient
    {
        public XMLFredClient(string apiKey, FredClientConfig config, IVintageComposer composer, HttpClient httpClient) : base(apiKey, config, composer, httpClient) { }

        protected override async Task<T> Parse<T>(string uri, string root)
        {
            Stream stream = await Download(uri);
         
            if (stream is null)
                return default(T);
            
            return (T)new XmlSerializer(typeof(T), new XmlRootAttribute(root)).Deserialize(stream);
        }
    }
}
