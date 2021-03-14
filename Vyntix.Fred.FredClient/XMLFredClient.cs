using LeaderAnalytics.Core.Serialization.XML;
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
    }
}
