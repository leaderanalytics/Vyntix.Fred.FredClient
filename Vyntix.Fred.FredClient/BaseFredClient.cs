﻿/*
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

// https://learn.microsoft.com/en-us/dotnet/core/extensions/http-ratelimiter

namespace LeaderAnalytics.Vyntix.Fred.FredClient;

public abstract class BaseFredClient : IFredClient
{
    public IDownloadJobStatistics JobStatistics { get; set; }               // Set by Autofac property injection
    public int RemainingLimitRequests { get; protected set; }               // Remaining requests since LastLimitReset
    protected ILogger<IFredClient> Logger { get; set; }
    protected readonly string API_key;
    public const string FRED_DATE_FORMAT = "yyyy-MM-dd";
    protected readonly FredClientConfig config;
    private HttpClient httpClient;
    private IVintageComposer composer;
    private SemaphoreSlim concurrentRequestThrottle;
    private BatchThrottleAsync batchThrottle;

    public BaseFredClient(string apiKey, FredClientConfig config, IVintageComposer composer, HttpClient httpClient, ILogger<IFredClient> logger)
    {

        API_key = "api_key=" + apiKey ?? throw new ArgumentNullException($"{nameof(apiKey)} can not be null.  Call UseAPIKey() when calling the FredClient service registration.  For example:  .AddFredClient().UseAPIKey(\"your API key here\") ");
        this.config = config ?? throw new ArgumentNullException(nameof(config));
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this.composer = composer ?? throw new ArgumentNullException(nameof(composer));

        if (httpClient.BaseAddress is null)
            throw new Exception($"{nameof(httpClient)} BaseAddress must be set.  The default value is {FredClientConfig.BaseAPIURL}");

        if (!httpClient.BaseAddress.OriginalString.EndsWith("/"))
            httpClient.BaseAddress = new Uri(httpClient.BaseAddress.ToString() + "/");

        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                    Logger.LogError("HttpStatusCode 404 received when accessing url: {uri}", uri);
                    break;
                }
                else
                {
                    int intCode = Convert.ToInt32(response.StatusCode);

                    if (intCode == 429) // Max requests exceeded
                        batchThrottle.BlockNow();
                    else
                    {
                        Logger.LogError("HttpStatusCode {code} received when accessing url: {uri}", intCode, uri);
                        wait = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Exception while downloading url: {uri}", uri);
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
            Logger.LogError("Max retry count exceeded when attempting to download url: {uri}", uri);

        return stream;
    }

    protected abstract Task<T> Parse<T>(string uri, string root) where T : class, new();
    
    protected abstract Task<List<Observation>> ParseObservations(string symbol, string uri);

    protected abstract Task<List<DateTime>> ParseVintageDates(string uri, string root);

    protected async Task<List<T>> Take<T>(string uriPrefix, string root) where T : class, new()
    {
        int skip = 0;
        int take = 1000;
        List<T> result = new List<T>(5000);

        while (true)
        {
            string uri = $"{uriPrefix}{(uriPrefix.Contains("?") ? "&" : "?")}offset={skip}&limit={take}";
            List<T> page = await Parse<List<T>>(uri, root);

            if (page?.Any() ?? false)
                result.AddRange(page);

            if ((page?.Count ?? 0) < take)
                break;

            skip += take;

        }
        return result;
    }

    #region Categories ------------------------

    public virtual async Task<Category> GetCategory(string categoryID)
    {
        string uri = "category?category_id=" + categoryID;
        return (await Parse<List<Category>>(uri, "categories"))?.FirstOrDefault();
    }

    public virtual async Task<List<Category>> GetCategoryChildren(string parentID)
    {
        string uri = "category/children?category_id=" + (parentID ?? throw new ArgumentNullException(nameof(parentID)));
        return (await Parse<List<Category>>(uri, "categories"))?.ToList();
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


    public virtual async Task<List<Series>> GetSeriesForCategory(string categoryID, bool includeDiscontinued)
    {
        string uri = "category/series?category_id=" + (categoryID ?? throw new ArgumentNullException(nameof(categoryID)));
        bool doIt = true;
        int offset = -1000;
        List<Series> result = new List<Series>(5000);

        while (doIt)
        {
            string newUri;

            offset += 1000;
            newUri = uri + "&offset=" + offset.ToString();
            List<Series> list = await Parse<List<Series>>(newUri, "seriess");

            if (list != null)
                result.AddRange(list.Where(x => includeDiscontinued || !(x.Title?.Contains("DISCONTINUED") ?? false)));

            if ((list?.Count ?? 0) < 1000)
                doIt = false;
        }
        return result.Any() ? result : null;
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

    public virtual async Task<List<Release>> GetAllReleases()
    {
        string uri = "releases";
        List<Release> releases = await Take<Release>(uri, "releases");
        return releases;
    }

    public virtual async Task<List<ReleaseDate>> GetAllReleaseDates(DateTime? realtimeStart, bool includeReleaseDatesWithNoData)
    {
        string uri = $"releases/dates?{(realtimeStart.HasValue ? "realtime_start=" + realtimeStart.Value.ToString(FRED_DATE_FORMAT) + "&" : null)}include_release_dates_with_no_data={(includeReleaseDatesWithNoData ? "true" : "false")}";
        List<ReleaseDate> releases = await Take<ReleaseDate>(uri, "release_dates");
        return releases;
    }

    public virtual async Task<Release> GetRelease(string nativeID)
    {
        ArgumentNullException.ThrowIfNull(nativeID);
        string uri = $"release?release_id={nativeID}";
        List<Release> releases = await Parse<List<Release>>(uri, "releases");
        return releases?.FirstOrDefault();
    }

    public virtual async Task<List<ReleaseDate>> GetReleaseDatesForRelease(string releaseNativeID, DateTime? realtimeStart, bool includeReleaseDatesWithNoData)
    {
        ArgumentNullException.ThrowIfNull(releaseNativeID);
        string uri = $"release/dates?release_id={releaseNativeID}&{(realtimeStart.HasValue ? "realtime_start=" + realtimeStart.Value.ToString(FRED_DATE_FORMAT) + "&" : null)}include_release_dates_with_no_data={(includeReleaseDatesWithNoData ? "true" : "false")}";
        List<ReleaseDate> releases = await Take<ReleaseDate>(uri, "release_dates");
        return releases;
    }

    public virtual async Task<List<ReleaseDate>> GetReleaseDates(string nativeReleaseID, int offset)
    {
        string uri = $"release/dates?release_id={(nativeReleaseID ?? throw new ArgumentNullException(nameof(nativeReleaseID)))}&include_release_dates_with_no_data=true&offset={offset}&sort_order=asc";
        List<ReleaseDate> releaseDates = await Parse<List<ReleaseDate>>(uri, "release_dates");
        return releaseDates?.ToList();
    }

    public virtual async Task<List<Source>> GetSourcesForRelease(string releaseNativeID)
    {
        ArgumentNullException.ThrowIfNull(releaseNativeID);
        string uri = $"release/sources?release_id={releaseNativeID}";
        List<Source> sources = await Take<Source>(uri, "sources");

        if (sources?.Any() ?? false)
            foreach (Source source in sources)
                source.SourceReleases = new List<SourceRelease> { new SourceRelease { SourceNativeID = source.NativeID, ReleaseNativeID = releaseNativeID } };
        
        return sources;
    }

    public virtual async Task<List<Release>> GetReleasesForSource(string nativeSourceID)
    {
        string uri = "source/releases?source_id=" + (nativeSourceID ?? throw new ArgumentNullException(nameof(nativeSourceID)));
        List<Release> releases = await Parse<List<Release>>(uri, "releases");
        return UpdateSourceNativeID(releases, nativeSourceID);
    }

    public virtual async Task<Release> GetReleaseForSeries(string symbol)
    {
        string uri = "series/release?series_id=" + (symbol ?? throw new ArgumentNullException(nameof(symbol)));
        List<Release> releases = await Parse<List<Release>>(uri, "releases");
        return releases.FirstOrDefault();
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
    
    public virtual async Task<List<Series>> GetSeriesForRelease(string releaseNativeID)
    {
        ArgumentNullException.ThrowIfNull(releaseNativeID);
        string uri = $"release/series?release_id={releaseNativeID}";
        List<Series> result = await Take<Series>(uri, "seriess");
        
        result?.ForEach(x => x.ReleaseID = releaseNativeID);
        return result;
    }

    private List<Release> UpdateSourceNativeID(List<Release> releases, string nativeSourceID)
    {
        if (releases?.Any() ?? false)
            foreach (Release release in releases)
                release.SourceReleases = new List<SourceRelease> { new SourceRelease { SourceNativeID = nativeSourceID, ReleaseNativeID = release.NativeID } };
        
        return releases;
    }

    #endregion

    #region Observations -------------------------------------------------

    public virtual async Task<List<Observation>> GetObservations(string symbol) =>
        await GetObservations(symbol, DataDensity.Sparse);

    public virtual async Task<List<Observation>> GetObservations(string symbol, DataDensity density) => 
        await GetObservations(symbol, await GetVintageDates(symbol), density);

    public async Task<List<Observation>> GetObservations(string symbol, DateTime obsPeriod,  DateTime? RTStart, DateTime? RTEnd, DataDensity density)
    {
        List<Observation> allObservations = await GetObservations(symbol, obsPeriod, obsPeriod, density);
        
        if (!allObservations?.Any() ?? false)
            return allObservations;
        else if (RTStart is null && RTEnd is null) 
            return allObservations;
        else if(RTStart is null)
            return allObservations.Where(x => x.VintageDate <= RTEnd.Value).ToList();

        allObservations = allObservations.OrderBy(x => x.VintageDate).ToList(); 

        // find the first vintage that is greater than the realtime start and get the value
        // get the value of the vintage before it.  Keep looking at previous vintages till we find the first one with that value.

        // Find the the largest date that is less than or equal to the real-time start
        Observation startVintage = allObservations.Where(x => x.VintageDate <= RTStart.Value).Last();

        // Search backwards through vintages while the observation value is the same as startVintage.Value.  
        // The real startVintage is the first vintage that has the same Observation.Value
        // startVintage = allObservations.OrderByDescending(x => x.VintageDate).Where(x => x.VintageDate <=startVintage.VintageDate && x.Value == startVintage.Value).Last();

        for (int i = allObservations.IndexOf(startVintage); i > 0; i--)
        {
            if (allObservations[i - 1].Value == startVintage.Value)
                startVintage = allObservations[i - 1];
            else
                break;
        }

        List<Observation> result = allObservations.Where(x => x.VintageDate >= startVintage.VintageDate && x.VintageDate <= RTEnd.Value).ToList();
        return result;
    }

    public async Task<List<Observation>> GetObservations(string symbol, DateTime? obsStart, DateTime? obsEnd, DataDensity density) =>
        await GetObservations(symbol, await GetVintageDates(symbol), obsStart, obsEnd, density);

    public virtual async Task<List<Observation>> GetObservations(string symbol, IList<DateTime> vintageDates, DataDensity density) =>
        await GetObservations(symbol, vintageDates, null, null, density);

    public virtual async Task<List<Observation>> GetObservations(string symbol, IList<DateTime> vintageDates, DateTime? obsStart, DateTime? obsEnd, DataDensity density)
    {
        if (string.IsNullOrEmpty(symbol))
            throw new ArgumentNullException(nameof(symbol));

        if (!(vintageDates?.Any()?? false))
            throw new Exception("vintageDates argument can not be null and must contain at least one vintage date.  Make sure symbol is valid.");

        List<Observation> result = new List<Observation>(10000);
        List<Observation> obs;
        int skip = 0;
        int take = 50;
        DataDensity tmpDensity = density;
        List<Task<List<Observation>>> tasks = new List<Task<List<Observation>>>(vintageDates.Count / take);

        while (skip < vintageDates.Count)
        {
            string[] dates = vintageDates.Skip(skip).Take(take).Select(x => x.ToString(FRED_DATE_FORMAT)).ToArray();
            string sdates = String.Join(",", dates);
            string uri = $"series/observations?series_id={symbol}&vintage_dates={sdates}&output_type={(density == DataDensity.Dense ? "2" : "3")}";
            
            if (obsStart.HasValue)
                uri += $"&observation_start={obsStart.Value.ToString(FRED_DATE_FORMAT)}";

            if (obsEnd.HasValue)
                uri += $"&observation_end={obsEnd.Value.ToString(FRED_DATE_FORMAT)}";

            tasks.Add(ParseObservations(symbol, uri)); 
            skip += take;
        }
        
        await Task.WhenAll(tasks).ConfigureAwait(false);

        foreach (Task<List<Observation>> t in tasks)
        {
            if (t.IsFaulted)
                throw (new Exception("Error downloading vintages.  See inner exception.", t.Exception));

            if (t.Result is not null)
                result.AddRange(t.Result.Where(x => x.Value != "."));
        }
        
        return result;
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

    #region Vintages -----------------------------------------

    
    public virtual async Task<List<Vintage>> GetVintages(string symbol, DateTime? RTEnd = null)
    {
        List<Vintage> result = new(150);

        foreach (DateTime vintageDate in (await GetVintageDates(symbol, RTEnd)))
            result.Add(new Vintage { Symbol = symbol, VintageDate = vintageDate });

        return result;
    }
    

    public virtual async Task<List<DateTime>> GetVintageDates(string symbol, DateTime? RTEnd = null)
    {
        string uri = "series/vintagedates?series_id=" + (symbol ?? throw new ArgumentNullException(nameof(symbol)));

        // The question "What vintage dates were valid during some historical real-time period?" is invalid.
        // It is invalid because different vintages can be valid for different observations during the real-time period.
        // The question can only be asked when a single observation period is specified.
        // The correct question is: "What vintage dates were valid for a specific observation period during some historical real-time period?"
        

        if (RTEnd.HasValue)
            uri += $"&realtime_end={RTEnd.Value.Date.ToString(FRED_DATE_FORMAT)}";

        int offset = -5000;
        bool doIt = true;
        List<DateTime> result = new(150);
        List<DateTime> vintages = null;

        while (doIt)
        {
            string newUri;
            offset += 5000;
            newUri = uri + "&offset=" + offset.ToString();
            vintages = (await ParseVintageDates(newUri, "vintage_dates"))?.ToList();

            if (vintages != null)
                result.AddRange(vintages);
            else
                break;

            doIt = vintages.Count == 5000;
        }
        
        return result;
    }
    #endregion

    #region Sources ----------------------------------------------------------------------

    public virtual async Task<Source> GetSource(string sourceID)
    { 
        ArgumentNullException.ThrowIfNull(nameof(sourceID));
        string uri = $"source?source_id={sourceID}";
        return (await Parse<List<Source>>(uri, "sources")).FirstOrDefault();
    }

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
