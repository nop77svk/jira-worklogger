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
            .Cast<KeyValuePair<string?, string?>>()
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

    public UriQueryBuilder Add(string? key, int? value)
        => Add(key, value?.ToString());

    public UriQueryBuilder Add(string? key, long? value)
        => Add(key, value?.ToString());

    public UriQueryBuilder Add(string? key, decimal? value)
        => Add(key, value?.ToString());

    public override string ToString() => this.Any()
        ? '?' + string.Join('&', this
            .Where(x => !string.IsNullOrEmpty(x.Key) || !string.IsNullOrEmpty(x.Value))
            .Select(x => Uri.EscapeDataString(x.Key ?? string.Empty) + "=" + Uri.EscapeDataString(x.Value ?? string.Empty))
        )
        : string.Empty;

    public static implicit operator string(UriQueryBuilder self) => self.ToString();
}
