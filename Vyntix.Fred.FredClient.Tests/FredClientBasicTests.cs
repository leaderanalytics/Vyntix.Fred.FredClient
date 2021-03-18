using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeaderAnalytics.Vyntix.Fred.FredClient;
using NUnit.Framework;
using LeaderAnalytics.Vyntix.Fred.Model;

namespace LeaderAnalytics.Vyntix.Fred.FredClient.Tests
{
    [TestFixture(FredFileType.JSON)]
    [TestFixture(FredFileType.XML)]
    public class FredClientBasicTests : BaseTest
    {
        private const string DOES_NOT_EXIST = "DOES_NOT_EXIST"; // Arbitrary invalid identifier

        public FredClientBasicTests(FredFileType fileType) : base(fileType)
        {

        }

        [Test()]
        public async Task GetCategoriesForSeriesTest()
        {
            List<Category> data = await FredClient.GetCategoriesForSeries("EXJPUS");
            Assert.IsNotNull(data);

            data = await FredClient.GetCategoriesForSeries(DOES_NOT_EXIST);
            Assert.IsNull(data);
        }

        [Test()]
        public async Task GetCategoryTest()
        {
            Category data = await FredClient.GetCategory("125");
            Assert.IsNotNull(data);

            data = await FredClient.GetCategory(DOES_NOT_EXIST);
            Assert.IsNull(data);
        }

        [Test()]
        public async Task GetCategoryChildrenTest()
        {
            List<Category> data = await FredClient.GetCategoryChildren("13");
            Assert.IsNotNull(data);

            data = await FredClient.GetCategoryChildren(DOES_NOT_EXIST);
            Assert.IsNull(data);
        }

        [Test()]
        public async Task GetCategoryTagsTest()
        {
            List<CategoryTag> data = await FredClient.GetCategoryTags("125");
            Assert.IsNotNull(data);

            data = await FredClient.GetCategoryTags(DOES_NOT_EXIST);
            Assert.IsNull(data);
        }

        [Test()]
        public async Task GetObservationsTest()
        {
            List<Observation> data = await FredClient.GetObservations("GNPCA");
            Assert.IsNotNull(data);

            data = await FredClient.GetObservations(DOES_NOT_EXIST);
            Assert.IsNull(data);
        }

        [Test()]
        public async Task GetObservationsTest2()
        {
            List<Observation> data = await FredClient.GetObservations("GNPCA", new DateTime(2020, 1, 1), new DateTime(2020, 12, 31));
            Assert.IsNotNull(data);

            data = await FredClient.GetObservations(DOES_NOT_EXIST, new DateTime(2020, 1, 1), new DateTime(2020, 12, 31));
            Assert.IsNull(data);
        }

        [Test()]
        public async Task GetObservationsTest3()
        {
            List<DateTime> vintateDates = new List<DateTime>(10)
            {
                new DateTime(2020, 1, 1),
                new DateTime(2020, 1, 1),
                new DateTime(2020, 1, 1),
                new DateTime(2020, 1, 1),
                new DateTime(2020, 1, 1),
                new DateTime(2020, 1, 1),
                new DateTime(2020, 1, 1),
                new DateTime(2020, 1, 1),
                new DateTime(2020, 1, 1),
                new DateTime(2020, 1, 1)
            };
            List<Observation> data = await FredClient.GetObservations("GNPCA", vintateDates);
            Assert.IsNotNull(data);

            data = await FredClient.GetObservations(DOES_NOT_EXIST, vintateDates);
            Assert.IsNull(data);

        }

        [Test()]
        public async Task GetObservationUpdatesTest()
        {
            List<Observation> data = await FredClient.GetObservationUpdates("GNPCA", new DateTime(2020, 1, 1), new DateTime(2020, 12, 31));
            Assert.IsNotNull(data);

            data = await FredClient.GetObservationUpdates(DOES_NOT_EXIST, new DateTime(2020, 1, 1), new DateTime(2020, 12, 31));
            Assert.IsNull(data);
        }


        [Test()]
        public async Task GetRelatedCategoriesTest()
        {
            List<RelatedCategory> data = await FredClient.GetRelatedCategories("32073");
            Assert.IsNotNull(data);

            data = await FredClient.GetRelatedCategories(DOES_NOT_EXIST);
            Assert.IsNull(data);
        }


        [Test()]
        public async Task GetReleaseDatesTest()
        {
            List<ReleaseDate> data = await FredClient.GetReleaseDates("82", 0);
            Assert.IsNotNull(data);

            data = await FredClient.GetReleaseDates(DOES_NOT_EXIST, 0);
            Assert.IsNull(data);
        }


        [Test()]
        public async Task GetReleasesForSourceTest()
        {
            List<Release> data = await FredClient.GetReleasesForSource("1");
            Assert.IsNotNull(data);

            data = await FredClient.GetReleasesForSource(DOES_NOT_EXIST);
            Assert.IsNull(data);
        }

        [Test()]
        public async Task GetReleasesForSourceTest2()
        {
            List<Release> data = await FredClient.GetReleasesForSource("1", new DateTime(2020, 1, 1), new DateTime(2020, 12, 31));
            Assert.IsNotNull(data);

            data = await FredClient.GetReleasesForSource(DOES_NOT_EXIST, new DateTime(2020, 1, 1), new DateTime(2020, 12, 31));
            Assert.IsNull(data);
        }


        [Test()]
        public async Task GetSeriesTest()
        {
            Series data = await FredClient.GetSeries("GNPCA");
            Assert.IsNotNull(data);

            data = await FredClient.GetSeries(DOES_NOT_EXIST);
            Assert.IsNull(data);
        }

        [Test()]
        public async Task GetSeriesForCategoryTest()
        {
            List<SeriesCategory> data = await FredClient.GetSeriesForCategory("125", false);
            Assert.IsNotNull(data);

            data = await FredClient.GetSeriesForCategory(DOES_NOT_EXIST, false);
            Assert.IsNull(data);
        }

        [Test()]
        public async Task GetSeriesForReleaseTest()
        {
            List<Series> data = await FredClient.GetSeriesForRelease("51");
            Assert.IsNotNull(data);

            data = await FredClient.GetSeriesForRelease(DOES_NOT_EXIST);
            Assert.IsNull(data);
        }

        [Test()]
        public async Task GetSeriesTagsTest()
        {
            List<SeriesTag> data = await FredClient.GetSeriesTags("STLFSI");
            Assert.IsNotNull(data);

            data = await FredClient.GetSeriesTags(DOES_NOT_EXIST);
            Assert.IsNull(data);
        }

        [Test()]
        public async Task GetSourcesTest()
        {
            List<Source> data = await FredClient.GetSources();
            Assert.IsNotNull(data);
        }

        [Test()]
        public async Task GetSourcesTest1()
        {
            List<Source> data = await FredClient.GetSources(new DateTime(2020, 1, 1), new DateTime(2020, 12, 31));
            Assert.IsNotNull(data);
        }

        [Test()]
        public async Task GetVintgeDatesTest()
        {
            List<Vintage> data = await FredClient.GetVintgeDates("GNPCA", new DateTime(2020,1,1));
            Assert.IsNotNull(data);

            data = await FredClient.GetVintgeDates(DOES_NOT_EXIST, new DateTime(2020, 1, 1));
            Assert.IsNull(data);
        }
    }
}