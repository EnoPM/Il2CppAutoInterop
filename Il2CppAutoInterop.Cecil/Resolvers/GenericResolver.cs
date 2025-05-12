using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Il2CppAutoInterop.Cecil.Resolvers;

internal static class GenericResolver
{
    internal static bool TryResolveDllAssembly<T>(AssemblyName assemblyName,
        string directory,
        Func<string, T> loader,
        [MaybeNullWhen(false)] out T assembly) where T : class
    {
        assembly = null;

        var potentialDirectories = new List<string>
        {
            directory
        };

        if (!Directory.Exists(directory)) return false;

        potentialDirectories.AddRange(Directory.GetDirectories(directory, "*", SearchOption.AllDirectories));

        foreach (var subDirectory in potentialDirectories)
        {
            var potentialPaths = new[]
            {
                $"{assemblyName.Name}.dll", $"{assemblyName.Name}.exe"
            };
            foreach (var potentialPath in potentialPaths)
            {
                var path = Path.Combine(subDirectory, potentialPath);
                if (!File.Exists(path)) continue;
                try
                {
                    assembly = loader(path);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception while loading assembly: {ex.Message}");
                    continue;
                }
                return true;
            }
        }
        return false;
    }
}