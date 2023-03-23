namespace jwl.config;
using System.IO;
using System.Xml.Serialization;

public class ConfigLoader
{
    public ConfigLocationResolver LocationResolver { get; }

    public ConfigLoader(ConfigLocationResolver locationResolver)
    {
        LocationResolver = locationResolver;
    }

    public IEnumerable<(SymbolicConfigLocation, T)> LoadMultipleXml<T>(SymbolicConfigLocation locations, string fileName)
        where T : class
    {
        foreach (SymbolicConfigLocation singleLocation in Enum.GetValues<SymbolicConfigLocation>())
        {
            if ((locations & singleLocation) != 0)
            {
                string pathToConfigFile = Path.Combine(LocationResolver.Resolve(singleLocation), fileName);
                T configContents = LoadSingleXml<T>(pathToConfigFile);
                yield return (singleLocation, configContents);
            }
        }
    }

    private T LoadSingleXml<T>(Stream contents)
        where T : class
    {
        XmlSerializer serializer = new XmlSerializer(typeof(T));
        T? nullableResult = (T?)serializer.Deserialize(contents);
        if (nullableResult == null)
            throw new InvalidDataException(@"XML content deserialized to null");
        else
            return nullableResult;
    }

    private T LoadSingleXml<T>(string fileName)
        where T : class
    {
        using FileStream reader = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        try
        {
            return LoadSingleXml<T>(reader);
        }
        catch (Exception e)
        {
            throw new InvalidDataException($"Error loading XML content from \"{fileName}\"", e);
        }
        finally
        {
            reader.Close();
        }
    }
}
