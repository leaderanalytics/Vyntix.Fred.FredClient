using LeaderAnalytics.Vyntix.Fred.Domain;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Serilog;
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
            container.AddLogging(builder => builder.AddSerilog());
            container.AddFredClient();
            services = container.BuildServiceProvider();
        }


        [Test]
        public void Can_resolve_default_FredClient()
        {
            string apiKey = "secret";
            IServiceCollection container = new ServiceCollection();
            container.AddLogging(builder => builder.AddSerilog());
            container.AddFredClient().UseAPIKey(apiKey);
            IServiceProvider services = container.BuildServiceProvider();
            IFredClient fredClient = services.GetService<IFredClient>();
            Assert.IsTrue(fredClient is JsonFredClient);
        }

        [Test]
        public void Can_use_XML_filetype_FredClient()
        {
            string apiKey = "secret";
            IServiceCollection container = new ServiceCollection();
            container.AddFredClient().UseAPIKey(apiKey).UseFileType(FredFileType.XML);
            container.AddLogging(builder => builder.AddSerilog());
            IServiceProvider services = container.BuildServiceProvider();
            IFredClient fredClient = services.GetService<IFredClient>();
            Assert.IsTrue(fredClient is XMLFredClient);
        }

        [Test]
        public void Can_use_custom_config_FredClient()
        {
            string localhost = "https://localhost/";
            string apiKey = "secret";
            IServiceCollection container = new ServiceCollection();
            container.AddLogging(builder => builder.AddSerilog());
            container.AddFredClient().UseAPIKey(apiKey).UseConfig(x => new FredClientConfig { BaseURL = localhost });
            IServiceProvider services = container.BuildServiceProvider();
            IFredClient fredClient = services.GetService<IFredClient>();
            Assert.IsTrue(fredClient is JsonFredClient);
            Func<IServiceProvider, HttpClient> httpClientFactory = services.GetService<Func<IServiceProvider, HttpClient>>();
            HttpClient httpClient = httpClientFactory(services);
            Assert.AreEqual(localhost, httpClient.BaseAddress.AbsoluteUri);
        }

        [Test]
        public void Can_use_custom_composer_FredClient()
        {
            IVintageComposer composerMock = new Mock<IVintageComposer>().Object;
            string apiKey = "secret";
            IServiceCollection container = new ServiceCollection();
            container.AddLogging(builder => builder.AddSerilog());
            container.AddFredClient().UseAPIKey(apiKey).UseVintageComposer(x => composerMock);
            IServiceProvider services = container.BuildServiceProvider();
            IFredClient fredClient = services.GetService<IFredClient>();
            Assert.IsTrue(fredClient is JsonFredClient);
            Func<IServiceProvider, IVintageComposer> composerFactory = services.GetService<Func<IServiceProvider, IVintageComposer>>();
            IVintageComposer composer = composerFactory(services);
            Assert.IsTrue(composer.GetType().FullName == "Castle.Proxies.IVintageComposerProxy");
        }

        [Test]
        public void Can_resolve_multiple_instances_of_FredClient()
        {
            string apiKey = "secret";
            IServiceCollection container = new ServiceCollection();
            container.AddLogging(builder => builder.AddSerilog());
            container.AddFredClient().UseAPIKey(apiKey);
            IServiceProvider services = container.BuildServiceProvider();
            IFredClient fredClient_1 = services.GetService<IFredClient>();
            IFredClient fredClient_2 = services.GetService<IFredClient>();
            Assert.AreNotEqual(fredClient_1, fredClient_2);
        }
    }
}
