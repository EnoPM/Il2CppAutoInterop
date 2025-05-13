using Il2CppAutoInterop.BepInEx.Utils;
using Il2CppAutoInterop.Cecil.Interfaces;

namespace Il2CppAutoInterop.BepInEx.Extensions;

public static class AssemblyDependencyManagerExtensions
{
    public static void RegisterBepInExPlugin(this IAssemblyDependencyManager dependencyManager, string inputPath)
    {
        var baseBepInExDirectoryPath = BepInExUtility.FindBepInExDirectoryFromChildPath(inputPath);
        foreach (var directoryName in BepInExUtility.Directories)
        {
            dependencyManager.AddDirectory(Path.Combine(baseBepInExDirectoryPath, directoryName));
        }
    }
}