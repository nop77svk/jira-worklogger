namespace jwl.Jira.test;

using System.Xml.Serialization;

using jwl.Wadl;

public class WadlTests
    : IDisposable
{
    private readonly XmlSerializer _wadlSerializer;
    private readonly Stream _wadlResponseStream;
    private bool _disposedValue;

    public WadlTests()
    {
        _wadlSerializer = new XmlSerializer(typeof(WadlApplication));
        _wadlResponseStream = File.Open("_assets/ictime.wadl", FileMode.Open, FileAccess.Read);
    }

    [SetUp]
    public void Setup()
    {
        _wadlResponseStream.Seek(0, SeekOrigin.Begin);
    }

    [Test]
    public void CanDeserializeWadl()
    {
        Assert.IsNotNull(_wadlResponseStream);

        var wadlObj = _wadlSerializer.Deserialize(_wadlResponseStream);
        Assert.IsNotNull(wadlObj);
        Assert.IsInstanceOf(typeof(WadlApplication), wadlObj);

        var wadl = (WadlApplication?)wadlObj;
        Assert.IsNotNull(wadl);

        Assert.IsNotNull(wadl!.Resources);
    }

    [Test]
    public void CanFlattenWadl()
    {
        Assert.IsNotNull(_wadlResponseStream);
        var wadl = (WadlApplication?)_wadlSerializer.Deserialize(_wadlResponseStream);

        var flatWadl = wadl?.AsComposedWadlMethodDefinitionEnumerable().ToArray();
        Assert.IsNotNull(flatWadl);
        Assert.That(flatWadl?.Any() ?? false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _wadlResponseStream.Dispose();
            }

            _disposedValue = true;
        }
    }
}
