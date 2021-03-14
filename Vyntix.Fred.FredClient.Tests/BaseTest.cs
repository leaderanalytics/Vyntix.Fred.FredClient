﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LeaderAnalytics.Vyntix.Fred.Domain;
using LeaderAnalytics.Vyntix.Fred.FredClient;
using NUnit.Framework;

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
        }



        [SetUp]
        public void Setup()
        {
            HttpClient httpClient = new HttpClient() { BaseAddress = new Uri(FredClientConfig.BaseAPIURL) };

            if(CurrentFileType == FredFileType.XML)
                FredClient = new XMLFredClient(apiKey, new FredClientConfig(), new VintageComposer(), httpClient);
            else
                FredClient = new JsonFredClient(apiKey, new FredClientConfig(), new VintageComposer(), httpClient);
        }
    }
}