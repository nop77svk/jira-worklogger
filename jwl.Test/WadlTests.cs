namespace Jwl.Test;

using System.Xml.Serialization;

using Jwl.Wadl;

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
        Assert.That(_wadlResponseStream, Is.Not.Null);

        var wadlObj = _wadlSerializer.Deserialize(_wadlResponseStream);
        Assert.That(wadlObj, Is.Not.Null);
        Assert.That(wadlObj, Is.InstanceOf<WadlApplication>());
        var wadl = (WadlApplication?)wadlObj;
        Assert.That(wadl, Is.Not.Null);

        Assert.That(wadl?.Resources, Is.Not.Null);
    }

    [Test]
    public void CanFlattenWadl()
    {
        Assert.That(_wadlResponseStream, Is.Not.Null);
        var wadl = (WadlApplication?)_wadlSerializer.Deserialize(_wadlResponseStream);

        var flatWadl = wadl?.AsComposedWadlMethodDefinitionEnumerable().ToArray();
        Assert.That(flatWadl, Is.Not.Null);
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
