using Il2CppAutoInterop.BepInEx.Contexts;
using Il2CppAutoInterop.BepInEx.Interfaces;
using Il2CppAutoInterop.BepInEx.Processors.FileProcessors;

namespace Il2CppAutoInterop.BepInEx;

public static class BepInExIl2CppInterop
{
    public static void Run(IBepInExIl2CppInteropOptions options)
    {
        var commonContext = new PostProcessingContext(options);
        foreach (var assemblyFilePath in options.InputFilePaths)
        {
            var context = new BepInExPluginFileContext(commonContext, assemblyFilePath);
            var processor = new BepInExFileProcessor(context);
            processor.Load();
            processor.Process();
            if (options.UseVersionRandomizer)
            {
                processor.RandomizeAssemblyVersion();
            }
            if (options.OutputDirectoryPath == null)
            {
                var directoryPath = Path.GetDirectoryName(assemblyFilePath);
                if (directoryPath == null)
                {
                    throw new Exception($"Unable to get directory path for {assemblyFilePath}");
                }
                processor.Save(directoryPath);
            }
            else
            {
                processor.Save(options.OutputDirectoryPath);
            }
        }
    }
}