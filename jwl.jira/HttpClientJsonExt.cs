namespace jwl.jira;
using System.Text.Json;

public static class HttpClientJsonExt
{
    public static async Task<TResponse> GetJsonAsync<TResponse>(this HttpClient self, string? uri)
    {
        using Stream response = await self.GetStreamAsync(uri);
        TResponse result = await HttpClientJsonExt.DeserializeJsonStream<TResponse>(response);
        return result;
    }

    public static async Task<TResponse> GetJsonAsync<TResponse>(this HttpClient self, Uri? uri)
    {
        using Stream response = await self.GetStreamAsync(uri);
        TResponse result = await HttpClientJsonExt.DeserializeJsonStream<TResponse>(response);
        return result;
    }

    public static async Task<TResponse> DeserializeJsonStream<TResponse>(Stream stream)
    {
        JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IncludeFields = true
        };
        TResponse result = await JsonSerializer.DeserializeAsync<TResponse>(stream, jsonSerializerOptions)
            ?? throw new InvalidDataException("Cannot deserialize JSON");
        return result;
    }
}