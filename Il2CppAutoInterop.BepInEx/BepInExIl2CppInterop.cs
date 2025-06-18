using System.Diagnostics;
using Il2CppAutoInterop.BepInEx.Contexts;
using Il2CppAutoInterop.BepInEx.Interfaces;
using Il2CppAutoInterop.BepInEx.Processors.FileProcessors;
using Il2CppAutoInterop.BepInEx.Utils;
using Il2CppAutoInterop.Common.Logging;
using Il2CppAutoInterop.Core.Utils;

namespace Il2CppAutoInterop.BepInEx;

public sealed class BepInExIl2CppInterop
{
    public static void Run(IBepInExIl2CppInteropOptions options)
    {
        var runner = new BepInExIl2CppInterop(options);
        runner.Run();
        runner.LogProcessingSummary();
    }

    private readonly IBepInExIl2CppInteropOptions _options;
    private readonly PostProcessingContext _context;
    private readonly List<BepInExPluginFileProcessor> _processors = [];
    private readonly Stopwatch _stopwatch;

    private BepInExIl2CppInterop(IBepInExIl2CppInteropOptions options)
    {
        _stopwatch = new Stopwatch();
        _options = options;
        _context = new PostProcessingContext(_options);

        foreach (var assemblyFilePath in _context.AssemblyFilePaths)
        {
            var fileContext = new BepInExPluginFileContext(_context, assemblyFilePath);
            var processor = new BepInExPluginFileProcessor(fileContext);
            _processors.Add(processor);
        }
    }

    private void LogProcessingSummary()
    {
        var entries = GetProcessedFileNames();
        var assemblyText = entries.Count == 1 ? "assembly" : "assemblies";
        var fileNames = string.Join(", ", entries);
        var elapsedTime = $"{_stopwatch.Elapsed.Minutes:00}:{_stopwatch.Elapsed.Seconds:00}.{_stopwatch.Elapsed.Milliseconds:000}";
        var memoryUsed = GC.GetTotalMemory(false);
        var memoryUsedMb = (memoryUsed / 1024f / 1024f);
        
        Logger.Instance.Log($"Post compiled {entries.Count} {assemblyText} [{memoryUsedMb:0.00} Mo] ({fileNames}) in {elapsedTime}", ConsoleColor.Magenta);
    }

    private List<string> GetProcessedFileNames()
    {
        var fileNames = new List<string>();
        foreach (var processor in _processors)
        {
            if (processor.LoadedAssembly == null) continue;
            var fileName = Path.GetFileName(processor.LoadedAssembly.MainModule.FileName);
            fileNames.Add(fileName);
        }
        return fileNames;
    }

    private void Run()
    {
        _stopwatch.Start();
        var sortedProcessors = GetSortedProcessors();
        if (_context.Loader == null)
        {
            throw new Exception($"No loader found for {_context.BepInExDirectoryPath}");
        }
        _context.Loader.LoadDependencies();

        foreach (var processor in sortedProcessors)
        {
            Run(processor);
        }

        CleanUpUnityProjectDirectory();
        
        _stopwatch.Stop();
    }

    private void CleanUpUnityProjectDirectory()
    {
        if (_context.UnityProjectDirectoryPath == null) return;
        var generatedDirectory = UnityUtility.GetUnityEditorGeneratedDirectoryPath(_context.UnityProjectDirectoryPath);
        if (!Directory.Exists(generatedDirectory))
        {
            return;
        }
        
        var allowedDirectories = _context.InteropSummary.UnityProjectGeneratedFilePaths
            .Select(Path.GetDirectoryName)
            .ToHashSet();
        
        var allDirectories = Directory.GetDirectories(generatedDirectory, "*", SearchOption.AllDirectories);
        foreach (var directoryPath in allDirectories)
        {
            if (allowedDirectories.Contains(directoryPath)) continue;
            if (!Directory.Exists(directoryPath)) continue;
            Directory.Delete(directoryPath, true);
        }

        var allowedFiles = _context.InteropSummary.UnityProjectGeneratedFilePaths;
        
        var allFiles = Directory.GetFiles(generatedDirectory, "*.cs", SearchOption.AllDirectories);
        foreach (var filePath in allFiles)
        {
            if (allowedFiles.Contains(filePath)) continue;
            File.Delete(filePath);
            var metaFilePath = $"{filePath}.meta";
            if (!File.Exists(metaFilePath)) continue;
            File.Delete(metaFilePath);
        }
    }

    private void Run(BepInExPluginFileProcessor processor)
    {
        processor.Process();
        if (_options.UseVersionRandomizer)
        {
            processor.RandomizeAssemblyVersion();
        }
        if (_options.OutputDirectoryPath == null)
        {
            var directoryPath = Path.GetDirectoryName(processor.Context.InputPath);
            if (directoryPath == null)
            {
                throw new Exception($"Unable to get directory path for {processor.Context.InputPath}");
            }
            processor.Save(directoryPath);
        }
        else
        {
            processor.Save(_options.OutputDirectoryPath);
        }
    }

    private void LoadProcessor(BepInExPluginFileProcessor processor)
    {
        if (_context.Loader != null)
        {
            processor.Context.Loader = _context.Loader;
        }
        processor.Load();
        _context.Loader ??= processor.Context.Loader;
    }

    private List<BepInExPluginFileProcessor> GetSortedProcessors()
    {
        var sorter = new TopologicalSorter<BepInExPluginFileProcessor>(_processors, GetProcessorDependencies);
        return sorter.Sort();
    }

    private List<BepInExPluginFileProcessor> GetProcessorDependencies(BepInExPluginFileProcessor processor)
    {
        if (processor.LoadedAssembly == null)
        {
            LoadProcessor(processor);
        }
        var assembly = processor.LoadedAssembly;
        if (assembly == null)
        {
            Logger.Instance.Warning($"Assembly not loaded: '{processor.Context.InputPath}'.");
            return [];
        }

        var references = assembly.Modules
            .SelectMany(module => module.AssemblyReferences
                .Select(x => x.FullName))
            .ToHashSet();

        return _processors.Where(
                otherProcessor => otherProcessor.LoadedAssembly != null
                                  && references.Contains(otherProcessor.LoadedAssembly.FullName)
            )
            .ToList();
    }
}