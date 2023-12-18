namespace jwl.infra;
using System.Web;

public class UriQueryBuilder
    : List<KeyValuePair<string?, string?>>
{
    public UriQueryBuilder()
        : base()
    {
    }

    public UriQueryBuilder(string uriQuery)
        : base(HttpUtility.ParseQueryString(uriQuery)
            .AsEnumerable()
            .ToList())
    {
    }

    public UriQueryBuilder(IEnumerable<(string? Key, string? Value)> other)
        : base(other.Select(x => new KeyValuePair<string?, string?>(x.Key, x.Value)))
    {
    }

    public UriQueryBuilder(IEnumerable<KeyValuePair<string?, string?>> collection)
        : base(collection)
    {
    }

    public UriQueryBuilder(int capacity)
        : base(capacity)
    {
    }

    public UriQueryBuilder Add(string? key, string? value)
    {
        Add(new KeyValuePair<string?, string?>(key, value));
        return this;
    }

    public override string ToString() => "?" + string.Join('&', this.Select(x => Uri.EscapeDataString(x.Key ?? string.Empty) + "=" + Uri.UnescapeDataString(x.Key ?? string.Empty)));

    public static implicit operator string(UriQueryBuilder self) => self.ToString();
}