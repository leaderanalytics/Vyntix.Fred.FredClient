using LeaderAnalytics.Vyntix.Fred.Domain;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


namespace LeaderAnalytics.Vyntix.Fred.FredClient.Tests
{
    [TestFixture]
    public class ServiceCollectionTests
    {
        
        private readonly IServiceProvider services;

        public ServiceCollectionTests()
        {
            IServiceCollection container = new ServiceCollection();
            container.AddFredClient();
            services = container.BuildServiceProvider();
        }


        [Test]
        public void Can_resolve_default_FredClient_test()
        {
            string apiKey = "secret";
            IServiceCollection container = new ServiceCollection();
            container.AddFredClient().UseAPIKey(apiKey);
            IServiceProvider services = container.BuildServiceProvider();
            IFredClient fredClient = services.GetService<IFredClient>();
            Assert.IsTrue(fredClient is JSONFredClient);
            
        }

        [Test]
        public void Can_use_XML_filetype_FredClient_test()
        {
            string apiKey = "secret";
            IServiceCollection container = new ServiceCollection();
            container.AddFredClient().UseAPIKey(apiKey).UseFileType(FredFileType.XML);
            IServiceProvider services = container.BuildServiceProvider();
            IFredClient fredClient = services.GetService<IFredClient>();
            Assert.IsTrue(fredClient is XMLFredClient);
        }

        [Test]
        public void Can_use_custom_config_FredClient_test()
        {
            string localhost = "https://localhost/";
            string apiKey = "secret";
            IServiceCollection container = new ServiceCollection();
            container.AddFredClient().UseAPIKey(apiKey).UseConfig(x => new FredClientConfig { BaseURL = localhost });
            IServiceProvider services = container.BuildServiceProvider();
            IFredClient fredClient = services.GetService<IFredClient>();
            Assert.IsTrue(fredClient is JSONFredClient);
            Func<IServiceProvider, HttpClient> httpClientFactory = services.GetService<Func<IServiceProvider, HttpClient>>();
            HttpClient httpClient = httpClientFactory(services);
            Assert.AreEqual(localhost, httpClient.BaseAddress.AbsoluteUri);
        }

        [Test]
        public void Can_use_custom_composer_FredClient_test()
        {
            IVintageComposer composerMock = new Mock<IVintageComposer>().Object;
            string apiKey = "secret";
            IServiceCollection container = new ServiceCollection();
            container.AddFredClient().UseAPIKey(apiKey).UseVintageComposer(x => composerMock);
            IServiceProvider services = container.BuildServiceProvider();
            IFredClient fredClient = services.GetService<IFredClient>();
            Assert.IsTrue(fredClient is JSONFredClient);
            Func<IServiceProvider, IVintageComposer> composerFactory = services.GetService<Func<IServiceProvider, IVintageComposer>>();
            IVintageComposer composer = composerFactory(services);
            Assert.IsTrue(composer.GetType().FullName == "Castle.Proxies.IVintageComposerProxy");
        }
    }
}
