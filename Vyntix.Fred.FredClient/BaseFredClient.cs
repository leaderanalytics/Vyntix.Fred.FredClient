/*
 The MIT License (MIT)

Copyright (c) 2020 Leader Analytics

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

This copyright notice header shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

This software uses the FRED® API but is not endorsed or certified by the Federal Reserve Bank of St. Louis.
By using this software you agree to be bound by the FRED® API Terms of Use found here: https://fred.stlouisfed.org/legal/.

*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Serilog;
using LeaderAnalytics.Core.Threading;
using LeaderAnalytics.Vyntix.Fred.Domain;
using LeaderAnalytics.Vyntix.Fred.Model;
using System.Text;

namespace LeaderAnalytics.Vyntix.Fred.FredClient
{
    public abstract class BaseFredClient : IFredClient
    {
        public IDownloadJobStatistics JobStatistics { get; set; }               // Set by Autofac property injection
        public int RemainingLimitRequests { get; protected set; }               // Remaining requests since LastLimitReset
        protected readonly string API_key;
        protected const string FRED_DATE_FORMAT = "yyyy-MM-dd";
        protected readonly FredClientConfig config;
        private HttpClient httpClient;
        private IVintageComposer composer;
        private SemaphoreSlim concurrentRequestThrottle;
        private BatchThrottleAsync batchThrottle;

        public BaseFredClient(string apiKey, FredClientConfig config, IVintageComposer composer, HttpClient httpClient)
        {
            API_key = "api_key=" + apiKey ?? throw new ArgumentNullException($"{nameof(apiKey)} can not be null.  Call UseAPIKey() when calling the FredClient service registration.  For example:  .AddFredClient().UseAPIKey(\"your API key here\") ");
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.composer = composer ?? throw new ArgumentNullException(nameof(composer));

            if (httpClient.BaseAddress is null)
                throw new Exception($"{nameof(httpClient)} BaseAddress must be set.  The default value is {FredClientConfig.BaseAPIURL}");
            
            if(! httpClient.BaseAddress.OriginalString.EndsWith("/"))
                httpClient.BaseAddress = new Uri(httpClient.BaseAddress.ToString() + "/");
            
            concurrentRequestThrottle = new SemaphoreSlim(config.MaxConcurrentDownloads, config.MaxConcurrentDownloads);
            batchThrottle = new BatchThrottleAsync(120, 60000); // block when we reach the per-minute request limit.
        }

        protected virtual async Task<Stream> Download(string uri)
        {
            uri = uri + (uri.Contains("?") ? "&" : "?") + API_key;
            Stream stream = null;
            bool success = false;

            for (int retryCount = 0; retryCount < config.MaxDownloadRetries; retryCount++)
            {
                await concurrentRequestThrottle.WaitAsync();        // Limit entrance to MAX_CONCURRENT_DOWNLOADS 
                await batchThrottle.WaitForBatch();                 // Wait when we have reached API max request limit.
                bool wait = false;                                  // flag that allows us to first release concurrentRequestThrottle than wait for ERROR_DELAY.

                try
                {
                    JobStatistics?.IncrementTotalRequestCount(1);
                    JobStatistics?.IncrementActiveRequestCount(1);
                    var response = await httpClient.GetAsync(uri);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        stream = await response.Content.ReadAsStreamAsync();
                        success = true;
                        break;
                    }
                    else if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        Log.Error("HttpStatusCode 404 received when accessing url: {uri}", uri);
                        break;
                    }
                    else
                    {
                        int intCode = Convert.ToInt32(response.StatusCode);

                        if (intCode == 429) // Max requests exceeded
                            batchThrottle.BlockNow();
                        else
                        {
                            Log.Error("HttpStatusCode {code} received when accessing url: {uri}", intCode, uri);
                            wait = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Exception while downloading url: {uri}", uri);
                    wait = true;
                }
                finally
                {
                    concurrentRequestThrottle.Release();
                    JobStatistics?.IncrementActiveRequestCount(-1);

                    if (wait)
                        await Task.Delay(config.ErrorDelay);
                }
            }

            if (!success)
                Log.Error("Max retry count exceeded when attempting to download url: {uri}", uri);

            return stream;
        }
        

        protected abstract Task<T> Parse<T>(string uri, string root) where T : class, new();

        #region Categories ------------------------

        public virtual async Task<Category> GetCategory(string categoryID)
        {
            string uri = "category?category_id=" + categoryID;
            return (await Parse<List<Category>>(uri, "categories"))?.FirstOrDefault();
        }

        public virtual async Task<List<Category>> GetCategoryChildren(string parentID)
        {
            string uri = "category/children?category_id=" + (parentID ?? throw new ArgumentNullException(nameof(parentID)));
            return (await Parse<List<Category>>(uri, "categories")).ToList();
        }

        public virtual async Task<List<RelatedCategory>> GetRelatedCategories(string parentID)
        {
            string uri = "category/related?category_id=" + (parentID ?? throw new ArgumentNullException(nameof(parentID)));
            List<RelatedCategory> related = await Parse<List<RelatedCategory>>(uri, "categories");

            if (related?.Any() ?? false)
                related.ForEach(x => x.CategoryID = parentID);

            return related;
        }

        public virtual async Task<List<Category>> GetCategoriesForSeries(string symbol)
        {
            string uri = "series/categories?series_id=" + (symbol ?? throw new ArgumentNullException(nameof(symbol)));
            return (await Parse<List<Category>>(uri, "categories"));
        }

        public virtual async Task<List<CategoryTag>> GetCategoryTags(string categoryID)
        {
            string uri = "category/tags?category_id=" + (categoryID ?? throw new ArgumentNullException(nameof(categoryID)));
            List<CategoryTag> tags = (await Parse<List<CategoryTag>>(uri, "tags"));

            if (tags?.Any() ?? false)
                tags.ForEach(x => x.CategoryID = categoryID);

            return tags;
        }

        #endregion

        #region Series -----------------------------------

        public virtual async Task<List<SeriesCategory>> GetSeriesForCategory(string categoryID, bool includeDiscontinued)
        {
            string uri = "category/series?category_id=" + (categoryID ?? throw new ArgumentNullException(nameof(categoryID)));
            bool doIt = true;
            int offset = -1000;
            List<SeriesCategory> results = new List<SeriesCategory>(5000);
            
            while (doIt)
            {
                string newUri;

                offset += 1000;
                newUri = uri + "&offset=" + offset.ToString();
                List<Series> list = await Parse<List<Series>>(newUri, "seriess");

                if (list != null)
                    results.AddRange(list.Where(x => includeDiscontinued || !(x.Title?.Contains("DISCONTINUED") ?? false)).Select(x => new SeriesCategory { CategoryID = categoryID, Symbol = x.Symbol }));

                if ((list?.Count ?? 0) < 1000)
                    doIt = false;
            }
            return results;
        }

        public virtual async Task<Series> GetSeries(string symbol)
        {
            string uri = "series?series_id=" + (symbol ?? throw new ArgumentNullException(nameof(symbol)));
            return (await Parse<List<Series>>(uri, "seriess"))?.FirstOrDefault();
        }

        public virtual async Task<List<SeriesTag>> GetSeriesTags(string symbol)
        {
            string uri = "series/tags?series_id=" + (symbol ?? throw new ArgumentNullException(nameof(symbol)));
            List<SeriesTag> tags = (await Parse<List<SeriesTag>>(uri, "tags"));

            if (tags != null)
                tags.ForEach(x => x.Symbol = symbol);

            return tags;
        }

        #endregion

        #region Releases ----------------------------

        public virtual async Task<List<Release>> GetReleasesForSource(string nativeSourceID)
        {
            string uri = "source/releases?source_id=" + (nativeSourceID ?? throw new ArgumentNullException(nameof(nativeSourceID)));
            List<Release> releases = await Parse<List<Release>>(uri, "releases");
            return UpdateSourceNativeID(releases, nativeSourceID);
        }

        public virtual async Task<List<Release>> GetReleasesForSource(string nativeSourceID, DateTime RTStart, DateTime RTEnd)
        {
            string uri = "source/releases?source_id=" 
                + (nativeSourceID ?? throw new ArgumentNullException(nameof(nativeSourceID))) 
                + "&realtime_start=" + RTStart.Date.ToString(FRED_DATE_FORMAT)
                + "&realtime_end=" + RTEnd.Date.ToString(FRED_DATE_FORMAT);
            List<Release> releases = await Parse<List<Release>>(uri, "releases");
            return UpdateSourceNativeID(releases, nativeSourceID);
        }

        public virtual async Task<List<ReleaseDate>> GetReleaseDates(string nativeReleaseID, int offset)
        {
            string uri = $"release/dates?release_id={ (nativeReleaseID ?? throw new ArgumentNullException(nameof(nativeReleaseID))) }&include_release_dates_with_no_data=true&offset={offset}&sort_order=asc";
            List<ReleaseDate> releaseDates = await Parse<List<ReleaseDate>>(uri, "release_dates");
            return releaseDates.ToList();
        }

        public virtual async Task<List<Series>> GetSeriesForRelease(string releaseNativeID)
        {
            if (string.IsNullOrEmpty(releaseNativeID))
                throw new ArgumentException(nameof(releaseNativeID));

            int skip = 0;
            int take = 1000;
            List<Series> result = new List<Series>(5000);

            while (true)
            {
                string uri = $"release/series?release_id={releaseNativeID}&offset={skip}&limit={take}";
                List<Series> page = await Parse<List<Series>>(uri, "seriess");

                if (page?.Any() ?? false)
                    result.AddRange(page);

                if ((page?.Count ?? 0) < take)
                    break;

                skip += take;

            }
            result.ForEach(x => x.ReleaseID = releaseNativeID);
            return result;
        }

        private List<Release> UpdateSourceNativeID(List<Release> releases, string nativeSourceID)
        {
            releases?.ForEach(x => x.SourceNativeID = nativeSourceID);
            return releases;
        }

        #endregion

        #region Observations -------------------------------------------------

        public virtual async Task<List<Observation>> GetObservations(string symbol)
        {
            string uri = "series/observations?series_id=" + (symbol ?? throw new ArgumentNullException(nameof(symbol)));
            return UpdateSymbol((await Parse<List<Observation>>(uri, "observations")), symbol).ToList();
        }

        public virtual async Task<List<Observation>> GetObservations(string symbol, IList<DateTime> vintageDates)
        {
            if (string.IsNullOrEmpty(symbol))
                throw new ArgumentNullException(nameof(symbol));

            if (vintageDates == null)
                throw new ArgumentNullException(nameof(vintageDates));

            List<Observation> result = new List<Observation>(10000);
            List<IObservation> denseResult = new List<IObservation>(10000);
            int skip = 0;
            int take = 50;

            while (skip < vintageDates.Count)
            {
                string[] dates = vintageDates.Skip(skip).Take(take).Select(x => x.ToString(FRED_DATE_FORMAT)).ToArray();
                string sdates = String.Join(",", dates);
                string uri = "series/observations?series_id=" + symbol + "&vintage_dates=" + sdates;
                List<Observation> obs = (await Parse<List<Observation>>(uri, "observations"))?.Where(x => x.Value != ".").ToList(); // Remove this where clause when Observation.Value becomes nullable.;

                if (obs != null)
                {
                    /*
                     Why we have to call MakeDense here:
                     When downloading a chunk of vintage dates that is less than the full count of vintage dates, FRED will return sparse data
                     for THAT CHUNK.  This means that for EACH CHUNK the first vintage will be dense and every subsequent vintage will be sparse.
                     In order for MakeSparse to work, it must have dense data across all vintages - not a chunk that begins with
                     a dense vintage followed by sparse vintages.  The reason for this is that MakeSparse compares each vintage with
                     it's immediate predecessor in time.  It must have dense data for all vintages to determine if a value has changed.
                     If the preceding vintage is sparse and the current vintage is dense MakeSparse assumes observations for the current 
                     vintage are new.
                    */

                    if (vintageDates.Count <= take)
                        result.AddRange(UpdateSymbol(obs, symbol));
                    else
                        denseResult.AddRange(composer.MakeDense(obs.Cast<IObservation>().ToList()));
                }

                skip += take;
            }
            return vintageDates.Count <= take ? result : UpdateSymbol(composer.MakeSparse(denseResult).Cast<Observation>().ToList(), symbol).ToList();
        }


        public virtual async Task<List<Observation>> GetObservations(string symbol, DateTime RTStart, DateTime RTEnd)
        {
            string uri = "series/observations?series_id=" + (symbol ?? throw new ArgumentNullException(nameof(symbol)))
                + "&realtime_start=" + RTStart.Date.ToString(FRED_DATE_FORMAT)
                + "&realtime_end=" + RTEnd.Date.ToString(FRED_DATE_FORMAT);

            return UpdateSymbol((await Parse<List<Observation>>(uri, "observations")), symbol).ToList();
        }

        public virtual async Task<List<Observation>> GetObservationUpdates(string symbol, DateTime? ObsStart, DateTime? ObsEnd)
        {
            string uri = "series/observations?series_id=" + (symbol ?? throw new ArgumentNullException(nameof(symbol)));

            if (ObsStart.HasValue)
                uri += "&observation_start=" + ObsStart.Value.ToString(FRED_DATE_FORMAT);

            if (ObsEnd.HasValue)
                uri += "&observation_end=" + ObsEnd.Value.ToString(FRED_DATE_FORMAT);

            return UpdateSymbol((await Parse<List<Observation>>(uri, "observations")), symbol).ToList();
        }

        private IEnumerable<Observation> UpdateSymbol(IEnumerable<Observation> obs, string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
                throw new ArgumentNullException(nameof(symbol));

            if (obs == null)
                return null;

            foreach (IObservation x in obs)
                x.Symbol = symbol;

            return obs;
        }
        #endregion

        #region VintageDates -----------------------------------------

        public virtual async Task<List<Vintage>> GetVintgeDates(string symbol, DateTime? RTStart)
        {
            string uri = "series/vintagedates?series_id=" + (symbol ?? throw new ArgumentNullException(nameof(symbol)));

            if (RTStart != null)
                uri += "&realtime_start=" + RTStart.Value.Date.ToString(FRED_DATE_FORMAT);

            int offset = -10000;
            bool doIt = true;
            List<Vintage> results = new List<Vintage>(1500);
            List<Vintage> vintages = null;

            while (doIt)
            {
                string newUri;
                offset += 10000;
                newUri = uri + "&offset=" + offset.ToString();
                vintages = (await Parse<List<Vintage>>(newUri, "vintage_dates")).ToList();

                if (vintages != null)
                    results.AddRange(vintages);
                else
                    break;

                doIt = vintages.Count == 10000;
            }
            results.ForEach(x => x.Symbol = symbol);
            return results;
        }
        #endregion

        #region Sources ----------------------------------------------------------------------

        public virtual async Task<List<Source>> GetSources()
        {
            string uri = "sources";
            return (await Parse<List<Source>>(uri, "sources")).ToList();
        }

        public virtual async Task<List<Source>> GetSources(DateTime RTStart, DateTime RTEnd)
        {
            string rtStart = RTStart.Date.ToString(FRED_DATE_FORMAT);
            string rtEnd = RTEnd.Date.ToString(FRED_DATE_FORMAT);
            string uri = "sources" + "?realtime_start=" + rtStart + "&realtime_end=" + rtEnd;
            return (await Parse<List<Source>>(uri, "sources")).ToList();
        }

        #endregion
    }

}
