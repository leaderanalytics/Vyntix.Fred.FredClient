﻿namespace LeaderAnalytics.Vyntix.Fred.FredClient;

public static class FredClientServiceCollectionExtensions
{
    public static RegistrationValues RegistrationValues { get; set; }

    public static RegistrationValues AddFredClient(this IServiceCollection services)
    {
        RegistrationValues = new RegistrationValues(services);
        return RegistrationValues; 
    }
}

public class RegistrationValues
{
    private string apiKey { get; set; }
    private FredFileType fileType { get; set; }
    private IServiceCollection services;

    public RegistrationValues(IServiceCollection services)
    {
        this.services = services;
        Build();
    }

    public RegistrationValues UseAPIKey(string apiKey)
    {
        this.apiKey = apiKey;
        return this;
    }

    public RegistrationValues UseFileType(FredFileType fileType)
    {
        this.fileType = fileType;
        return this;
    }

    public RegistrationValues UseVintageComposer(Func<IServiceProvider, IVintageComposer> composerFactory)
    {
        services.AddSingleton<Func<IServiceProvider, IVintageComposer>>(x => composerFactory);
        return this;
    }

    public RegistrationValues UseHttpClient(Func<IServiceProvider, HttpClient> httpClientFactory)
    {
        services.AddSingleton<Func<IServiceProvider, HttpClient>>(x => httpClientFactory);
        return this;
    }

    public RegistrationValues UseConfig(Func<IServiceProvider, FredClientConfig> configFactory)
    {
        services.AddSingleton<Func<IServiceProvider, FredClientConfig>>(x => configFactory);
        return this;
    }

    private IServiceCollection Build()
    {
        services.AddSingleton<FredClientConfig>();
        services.AddSingleton<IVintageComposer, VintageComposer>();
        UseFileType(FredFileType.JSON);
        UseVintageComposer(x => x.GetService<IVintageComposer>());
        UseHttpClient(x =>
        {
            Func<IServiceProvider, FredClientConfig> configFactory = x.GetService<Func<IServiceProvider, FredClientConfig>>();
            FredClientConfig config = configFactory(x);
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(config.BaseURL);
            return httpClient;
        });
        UseConfig(x => x.GetService<FredClientConfig>());

        services.AddTransient<IFredClient>(x =>
        {
            Func<IServiceProvider, FredClientConfig> configFactory = x.GetService<Func<IServiceProvider, FredClientConfig>>();
            Func<IServiceProvider, IVintageComposer> composerFactory = x.GetService<Func<IServiceProvider, IVintageComposer>>();
            Func<IServiceProvider, HttpClient> httpClientFactory = x.GetService<Func<IServiceProvider, HttpClient>>();
            ILogger<IFredClient> logger = x.GetService<ILogger<IFredClient>>() ?? throw new Exception("A Logger could not resolved.  You must call AddLogging() when configuring IServiceCollection.  For example:  services.AddLogging(builder => builder.AddSerilog());");
            IFredClient fredClient = this.fileType == FredFileType.JSON ?
                new JsonFredClient(this.apiKey, configFactory(x), composerFactory(x), httpClientFactory(x), logger) :
                new XMLFredClient(this.apiKey, configFactory(x), composerFactory(x), httpClientFactory(x), logger);
            return fredClient;
        });

        return services;
    }
}
