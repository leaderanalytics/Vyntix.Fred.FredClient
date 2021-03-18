using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LeaderAnalytics.Vyntix.Fred.Domain;
using LeaderAnalytics.Vyntix.Fred.FredClient;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Serilog;

namespace LeaderAnalytics.Vyntix.Fred.FredClient.Tests
{
    public abstract class BaseTest
    {
        private readonly string apiKey;
        protected const string BaseURL = "https://api.stlouisfed.org/fred/";
        protected IFredClient FredClient;
        protected readonly FredFileType CurrentFileType;

        public BaseTest(FredFileType fileType)
        {
            CurrentFileType = fileType;
            string path = "C:\\Users\\sam\\OneDrive\\LeaderAnalytics\\Config\\Vyntix.Fred.FredClient\\apiKey.txt";
            apiKey = System.IO.File.ReadAllText(path);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Debug()
                .CreateLogger();

            Log.Information("Logging has been configured.");
        }



        [SetUp]
        public void Setup()
        {
            HttpClient httpClient = new HttpClient() { BaseAddress = new Uri(FredClientConfig.BaseAPIURL) };
            FredClientConfig config = new FredClientConfig { MaxDownloadRetries = 1 };
            ILoggerFactory loggerFactory = new LoggerFactory().AddSerilog();
            ILogger<IFredClient> logger = loggerFactory.CreateLogger<IFredClient>();

            if(CurrentFileType == FredFileType.XML)
                FredClient = new XMLFredClient(apiKey, config, new VintageComposer(), httpClient, logger);
            else
                FredClient = new JsonFredClient(apiKey, config, new VintageComposer(), httpClient, logger);
        }
    }
}
