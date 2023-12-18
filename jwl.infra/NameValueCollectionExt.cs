namespace jwl.infra;
using System.Collections.Specialized;

public static class NameValueCollectionExt
{
    public static IEnumerable<KeyValuePair<string?, string?>> AsEnumerable(this NameValueCollection self)
    {
        foreach (string? key in self.AllKeys)
        {
            yield return new KeyValuePair<string?, string?>(key, self[key]);
        }
    }
}