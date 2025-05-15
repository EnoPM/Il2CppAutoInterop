using Il2CppAutoInterop.BepInEx.Utils;
using Il2CppAutoInterop.Cecil.Interfaces;

namespace Il2CppAutoInterop.BepInEx.Extensions;

public static class AssemblyDependencyManagerExtensions
{
    public static void RegisterBepInExPlugin(this IAssemblyDependencyManager dependencyManager, string baseBepInExDirectoryPath, string inputPath)
    {
        dependencyManager.AddDirectory(Path.Combine(Path.GetDirectoryName(baseBepInExDirectoryPath)!, "dotnet"));
        foreach (var directoryName in BepInExUtility.Directories)
        {
            dependencyManager.AddDirectory(Path.Combine(baseBepInExDirectoryPath, directoryName));
        }
    }
}