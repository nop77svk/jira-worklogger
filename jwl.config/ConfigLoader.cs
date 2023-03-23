namespace jwl.config;
using System.IO;
using System.Xml.Serialization;

public static class ConfigLoader
{
    public static IEnumerable<(string, T)> LoadMultipleXml<T>(IEnumerable<string> folders, string fileName)
        where T : class
    {
        foreach (string singleFolder in folders)
        {
            string fullFilePath = Path.Combine(singleFolder, fileName);
            T fileContents = LoadSingleXml<T>(fullFilePath);
            yield return (singleFolder, fileContents);
        }
    }

    public static T LoadSingleXml<T>(Stream contents)
        where T : class
    {
        XmlSerializer serializer = new XmlSerializer(typeof(T));
        T? nullableResult = (T?)serializer.Deserialize(contents);
        if (nullableResult == null)
            throw new InvalidDataException(@"XML content deserialized to null");
        else
            return nullableResult;
    }

    public static T LoadSingleXml<T>(string fileName)
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
