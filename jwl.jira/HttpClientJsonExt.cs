namespace jwl.jira;
using System.Text.Json;

public static class HttpClientJsonExt
{
    public static async Task<TResponse> GetAsJsonAsync<TResponse>(this HttpClient self, string uri)
    {
        using Stream response = await self.GetStreamAsync(uri);
        TResponse result = await DeserializeJsonStreamAsync<TResponse>(response);
        return result;
    }

    public static async Task<TResponse> GetAsJsonAsync<TResponse>(this HttpClient self, Uri uri)
    {
        using Stream response = await self.GetStreamAsync(uri);
        TResponse result = await DeserializeJsonStreamAsync<TResponse>(response);
        return result;
    }

    public static async Task<TResponse> DeserializeJsonStreamAsync<TResponse>(Stream stream)
    {
        JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IncludeFields = true
        };
        TResponse result = await JsonSerializer.DeserializeAsync<TResponse>(stream, jsonSerializerOptions)
            ?? throw new NullReferenceException("JSON deserialization NULL result");
        return result;
    }
}