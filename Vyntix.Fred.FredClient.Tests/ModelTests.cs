using NUnit.Framework;
using LeaderAnalytics.Vyntix.Fred.Model;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;
using System;

namespace LeaderAnalytics.Vyntix.Fred.FredClient.Tests
{
    [TestFixture(FredFileType.JSON)]
    [TestFixture(FredFileType.XML)]
    public class ModelTests : BaseTest
    {
        public ModelTests(FredFileType fileType) : base(fileType)
        { 
        }

        private bool IsZeroString(string s) => String.IsNullOrEmpty(s) || s == "0";

        [Test]
        public async Task CategoryTest()
        {
            Category data = await FredClient.GetCategory("125");
            Assert.IsNotNull(data);
            Assert.IsFalse(IsZeroString(data.NativeID));
            Assert.IsFalse(String.IsNullOrEmpty(data.Name));
            Assert.IsFalse(IsZeroString(data.ParentID));
        }

        [Test]
        public async Task ObservationTest()
        {
            Observation data = (await FredClient.GetObservations("GNPCA")).FirstOrDefault();
            Assert.IsNotNull(data);
            Assert.IsFalse(String.IsNullOrEmpty(data.Symbol));
            Assert.IsFalse(String.IsNullOrEmpty(data.Value));
            Assert.AreNotEqual(DateTime.MinValue, data.ObsDate);
            Assert.AreNotEqual(DateTime.MinValue, data.VintageDate);
        }

        [Test]
        public async Task RelatedCategoryTest()
        {
            RelatedCategory data = (await FredClient.GetRelatedCategories("32073")).FirstOrDefault();
            Assert.IsNotNull(data);
            Assert.IsFalse(IsZeroString(data.CategoryID));
            Assert.IsFalse(IsZeroString(data.RelatedCategoryID));
        }

        [Test]
        public async Task ReleaseTest()
        {
            Release data = (await FredClient.GetReleasesForSource("1")).FirstOrDefault();
            Assert.IsNotNull(data);
            Assert.IsFalse(IsZeroString(data.NativeID));
            Assert.IsFalse(String.IsNullOrEmpty(data.Name));
            Assert.AreNotEqual(DateTime.MinValue, data.RTStart);
        }

        [Test]
        public async Task ReleaseDateTest()
        {
            ReleaseDate data = (await FredClient.GetReleaseDates("82", 0)).FirstOrDefault();
            Assert.IsNotNull(data);
            Assert.IsFalse(IsZeroString(data.ReleaseID));
            Assert.AreNotEqual(DateTime.MinValue, data.DateReleased);
        }

        [Test]
        public async Task SeriesTest()
        {
            Series data = (await FredClient.GetSeries("GNPCA"));
            Assert.IsNotNull(data);
            Assert.IsFalse(String.IsNullOrEmpty(data.Symbol));
            Assert.IsFalse(String.IsNullOrEmpty(data.Title));
            Assert.IsFalse(String.IsNullOrEmpty(data.Frequency));
            Assert.IsFalse(String.IsNullOrEmpty(data.Units));
            Assert.IsFalse(String.IsNullOrEmpty(data.SeasonalAdj));
        }

        [Test]
        public async Task SeriesCategoryTest()
        {
            SeriesCategory data = (await FredClient.GetSeriesForCategory("125", false)).FirstOrDefault();
            Assert.IsNotNull(data);
            Assert.IsFalse(String.IsNullOrEmpty(data.Symbol));
            Assert.IsFalse(IsZeroString(data.CategoryID));
        }

        [Test]
        public async Task SeriesTagTest()
        {
            SeriesTag data = (await FredClient.GetSeriesTags("STLFSI")).FirstOrDefault();
            Assert.IsNotNull(data);
            Assert.IsFalse(String.IsNullOrEmpty(data.Symbol));
            Assert.IsFalse(String.IsNullOrEmpty(data.Name));
            Assert.IsFalse(String.IsNullOrEmpty(data.GroupID));
            Assert.IsFalse(String.IsNullOrEmpty(data.Notes));
            Assert.Greater(data.Popularity, 0);
        }

        [Test]
        public async Task SourceTest()
        {
            Source data = (await FredClient.GetSources()).FirstOrDefault();
            Assert.IsNotNull(data);
            Assert.IsFalse(String.IsNullOrEmpty(data.NativeID));
            Assert.IsFalse(String.IsNullOrEmpty(data.Name));
            Assert.IsFalse(String.IsNullOrEmpty(data.Link));
        }

        [Test]
        public async Task VintageTest()
        {
            Vintage data = (await FredClient.GetVintageDates("GNPCA", new DateTime(2020, 1, 1))).FirstOrDefault();
            Assert.IsNotNull(data);
            Assert.IsFalse(String.IsNullOrEmpty(data.Symbol));
            Assert.AreNotEqual(DateTime.MinValue, data.VintageDate);
        }
    }
}