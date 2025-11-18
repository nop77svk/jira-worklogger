namespace jwl.Infra;

using System;
using System.Reflection;

public static class AssemblyVersioning
{
    public static Version? GetExeVersion()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        AssemblyName assemblyName = assembly.GetName();
        return assemblyName.Version;
    }

    public static Version? GetCoreVersion(Type coreType)
    {
        Assembly? assembly = Assembly.GetAssembly(coreType);
        AssemblyName? assemblyName = assembly?.GetName();
        return assemblyName?.Version;
    }
}
