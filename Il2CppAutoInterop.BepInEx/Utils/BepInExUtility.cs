namespace Il2CppAutoInterop.BepInEx.Utils;

internal static class BepInExUtility
{
    private const string BepInExDirectoryName = "BepInEx";
    internal static readonly string[] Directories = ["core", "plugins", "interop", "patchers", "dotnet"];
    
    internal static string FindBepInExDirectoryFromChildPath(string path)
    {
        var exists = true;
        while (exists)
        {
            var isFile = File.Exists(path);
            var isDirectory = !isFile && Directory.Exists(path);
            exists = isFile || isDirectory;
            if (!exists) break;
            if (IsBepInExDirectory(path))
            {
                return path;
            }
            var parent = Path.GetDirectoryName(path);
            if (parent == null) break;
            path = parent;
        }

        throw new DirectoryNotFoundException(path);
    }
    
    private static string[] GetBepInExDirectoriesAbsolutePaths(string basePath)
    {
        return Directories.Select(directory => Path.Combine(basePath, directory)).ToArray();
    }
    
    private static bool IsBepInExDirectory(string directoryPath)
    {
        var name = Path.GetFileName(directoryPath);
        if (name != BepInExDirectoryName) return false;
        var bepInExDirectories = GetBepInExDirectoriesAbsolutePaths(directoryPath);
        var subDirectories = Directory.GetDirectories(directoryPath).ToList();
        return subDirectories.Count != 0 && bepInExDirectories.All(subDirectories.Contains);
    }
}