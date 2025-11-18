namespace jwl.Core;

using System;

internal class ConfigDrivenHttpClientFactory
    : IDisposable
{
    private readonly Lazy<HttpClientHandler> _lazyHttpClientHandler;
    private readonly Lazy<HttpClient> _lazyHttpClient;
    private bool _isDisposed;

    public HttpClientHandler HttpClientHandler => _lazyHttpClientHandler.Value;
    public HttpClient HttpClient => _lazyHttpClient.Value;

    public ConfigDrivenHttpClientFactory(AppConfig config)
    {
        _lazyHttpClientHandler = new Lazy<HttpClientHandler>(() => InstantiateHttpClientHandler(config));
        _lazyHttpClient = new Lazy<HttpClient>(() => InstantiateHttpClient(config));
    }

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
                if (_lazyHttpClient.IsValueCreated)
                {
                    _lazyHttpClient.Value.Dispose();
                }

                if (_lazyHttpClientHandler.IsValueCreated)
                {
                    _lazyHttpClientHandler.Value.Dispose();
                }
            }

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
