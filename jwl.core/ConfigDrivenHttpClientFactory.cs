namespace jwl.Core;

using System;

internal class ConfigDrivenHttpClientFactory
    : IDisposable
{
    public HttpClientHandler HttpClientHandler => _lazyHttpClientHandler.Value;
    public HttpClient HttpClient => _lazyHttpClient.Value;

    private readonly Lazy<HttpClientHandler> _lazyHttpClientHandler;
    private readonly Lazy<HttpClient> _lazyHttpClient;
    private bool _isDisposed;

    public ConfigDrivenHttpClientFactory(AppConfig config)
    {
        _lazyHttpClientHandler = new Lazy<HttpClientHandler>(() => InstantiateHttpClientHandler(config));
        _lazyHttpClient = new Lazy<HttpClient>(() => InstantiateHttpClient(config));
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~ConfigDrivenHttpClientFactory()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            if (_lazyHttpClient.IsValueCreated)
                _lazyHttpClient.Value.Dispose();

            if (_lazyHttpClientHandler.IsValueCreated)
                _lazyHttpClientHandler.Value.Dispose();

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _isDisposed = true;
        }
    }

    private HttpClientHandler InstantiateHttpClientHandler(AppConfig config)
    {
        HttpClientHandler result = new HttpClientHandler()
        {
            UseProxy = config.JiraServer?.UseProxy ?? false,
            UseDefaultCredentials = false,
            MaxConnectionsPerServer = config.JiraServer?.MaxConnectionsPerServer ?? AppConfigFactory.DefaultMaxConnectionsPerServer
        };

        if (config.JiraServer?.SkipSslCertificateCheck ?? false)
            HttpClientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;

        return result;
    }

    private HttpClient InstantiateHttpClient(AppConfig config)
    {
        return new HttpClient(HttpClientHandler)
        {
            BaseAddress = new Uri(config.JiraServer?.BaseUrl ?? string.Empty)
        };
    }
}
