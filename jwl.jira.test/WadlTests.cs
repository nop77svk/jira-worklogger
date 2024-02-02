namespace jwl.jira.test;

using jwl.infra;
using jwl.jira.ictime;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Serialization;

public class WadlTests
    : IDisposable
{
    private readonly HttpClientHandler _httpClientHandler;
    private readonly HttpClient _httpClient;
    private readonly XmlSerializerFactory _serializerFactory;
    private readonly XmlSerializer _wadlSerializer;
    private Stream? _wadlResponseStream;

    public WadlTests()
    {
        _httpClientHandler = new HttpClientHandler()
        {
            UseProxy = false,
            UseDefaultCredentials = false,
            MaxConnectionsPerServer = 4,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };

        var secrets = new ConfigurationBuilder().AddUserSecrets<ConfigSecrets>().Build();
        var provider = secrets.Providers.First();
        provider.TryGet("JiraUserName", out string jiraUserName);
        provider.TryGet("JiraUserPassword", out string jiraUserPassword);

        _httpClient = new HttpClient(_httpClientHandler)
        {
            BaseAddress = new Uri("https://jira.whitestein.com/rest/ictime/1.0/"),
        };
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(@"Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(jiraUserName + ":" + jiraUserPassword)));

        _serializerFactory = new XmlSerializerFactory();
        _wadlSerializer = _serializerFactory.CreateSerializer(typeof(WadlApplication));
    }

    [SetUp]
    public void Setup()
    {
        _wadlResponseStream = _httpClient.GetStreamAsync("application.wadl").Result;
    }

    [TearDown]
    public void TearDown()
    {
        _wadlResponseStream?.Dispose();
    }

    [Test]
    public void CanDeserializeWadl()
    {
        Assert.IsNotNull(_wadlResponseStream);

        var wadlObj = _wadlSerializer.Deserialize(_wadlResponseStream);
        Assert.IsNotNull(wadlObj);
        Assert.IsInstanceOf(typeof(WadlApplication), wadlObj);
        var wadl = (WadlApplication)wadlObj;
        Assert.IsNotNull(wadl);

        Assert.IsNotNull(wadl.Resources);
    }

    [Test]
    public void CanFlattenWadl()
    {
        Assert.IsNotNull(_wadlResponseStream);
        var wadl = (WadlApplication?)_wadlSerializer.Deserialize(_wadlResponseStream);

        var flatWadl = wadl?.AsEnumerable().ToArray();
        Assert.IsNotNull(flatWadl);
        Assert.That(flatWadl.Any());
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _httpClientHandler.Dispose();
    }
}