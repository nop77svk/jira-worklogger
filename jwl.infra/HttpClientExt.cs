namespace jwl.infra;
using System.Text.Json;
using System.Xml.Serialization;

public static class HttpClientExt
{
    private static readonly XmlSerializerFactory _serializerFactory = new XmlSerializerFactory();

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

    public static async Task<TResponse> DeserializeXmlStreamAsync<TResponse>(Stream responseContentStream)
    {
        XmlSerializer xmlSerializer = _serializerFactory.CreateSerializer(typeof(TResponse));
        Task<object> deserializerTask = Task.Factory.StartNew(() =>
        {
            object result = xmlSerializer.Deserialize(responseContentStream)
                ?? throw new NullReferenceException("XML deserialization NULL result");

            return result;
        });

        TResponse result = (TResponse)await deserializerTask;
        return result;
    }
}