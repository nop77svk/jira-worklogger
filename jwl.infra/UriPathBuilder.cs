namespace jwl.Infra;

public class UriPathBuilder
    : List<string>
{
    public UriPathBuilder(string uriPath)
        : base(uriPath
            .Split('/')
            .Select(folder => Uri.UnescapeDataString(folder))
            .Where(folder => !string.IsNullOrEmpty(folder))
        )
    {
    }

    public UriPathBuilder()
        : base()
    {
    }

    public UriPathBuilder(IEnumerable<string> collection)
        : base(collection)
    {
    }

    public UriPathBuilder(int capacity)
        : base(capacity)
    {
    }

    public static implicit operator string(UriPathBuilder self) => self.ToString();

    public override string ToString() => string.Join('/', this.Select(x => Uri.EscapeDataString(x)));

    public new UriPathBuilder Add(string folder)
    {
        base.Add(folder);
        return this;
    }
}