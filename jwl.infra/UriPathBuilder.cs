namespace jwl.infra;

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

    public override string ToString() => string.Join('/', this.Select(x => Uri.EscapeDataString(x)));

    public static implicit operator string(UriPathBuilder self) => self.ToString();

    public new UriPathBuilder Add(string folder)
    {
        base.Add(folder);
        return this;
    }
}
