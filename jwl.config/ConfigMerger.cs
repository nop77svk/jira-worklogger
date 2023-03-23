namespace jwl.config;

public static class ConfigMerger
{
    public static T? MergeObjects<T>(IEnumerable<T> objects)
        where T : class
    {
        T? result = null;

        foreach (T obj in objects)
        {
            if (result == null)
            {
                result = obj;
            }
            else
            {
                // 2do! merge result (higher prio) with obj (lower prio) via automapper
            }
        }

        return result;
    }
}
