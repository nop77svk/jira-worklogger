namespace jwl.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class ConfigDrivenHttpClientFactory
    : IDisposable
{
    public HttpClientHandler HttpClientHandler => _lazyHttpClientHandler.Value;
    public HttpClient HttpClient => _lazyHttpClient.Value;

    private readonly AppConfig _config;
    private readonly Lazy<HttpClientHandler> _lazyHttpClientHandler;
    private readonly Lazy<HttpClient> _lazyHttpClient;
    private bool _isDisposed;

    public ConfigDrivenHttpClientFactory(AppConfig config)
    {
        _config = config;
        _lazyHttpClientHandler = new Lazy<HttpClientHandler>(() => InstantiateHttpClientHandler());
        _lazyHttpClient = new Lazy<HttpClient>(() => InstantiateHttpClient());
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

    private HttpClientHandler InstantiateHttpClientHandler()
    {
        HttpClientHandler result = new HttpClientHandler()
        {
            UseProxy = _config.JiraServer?.UseProxy ?? false,
            UseDefaultCredentials = false,
            MaxConnectionsPerServer = _config.JiraServer?.MaxConnectionsPerServer ?? AppConfigFactory.DefaultMaxConnectionsPerServer
        };

        if (_config.JiraServer?.SkipSslCertificateCheck ?? false)
            HttpClientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;

        return result;
    }

    private HttpClient InstantiateHttpClient()
    {
        return new HttpClient(HttpClientHandler)
        {
            BaseAddress = new Uri(_config.JiraServer?.BaseUrl ?? string.Empty)
        };
    }
}
