﻿using System.Reflection;
using Il2CppAutoInterop.Cecil.Extensions;
using Il2CppAutoInterop.Cecil.Interfaces;
using Mono.Cecil;

namespace Il2CppAutoInterop.Core;

public sealed class AssemblyDependencyManager(AssemblyLoader loader) : IAssemblyDependencyManager
{
    public IList<IDependencyFile> Files { get; } = new List<IDependencyFile>();

    public void AddFile(params string[] files)
    {
        var existingPaths = GetExistingPaths();
        AddFiles(existingPaths, files);
    }

    public void AddDirectory(params string[] directories)
    {
        var existingPaths = GetExistingPaths();
        foreach (var directoryPath in directories)
        {
            var assemblyPaths = Directory.GetFiles(directoryPath, "*.dll", SearchOption.AllDirectories);
            AddFiles(existingPaths, assemblyPaths);
        }
    }

    public void LoadAllFiles()
    {
        foreach (var dependency in Files)
        {
            if (dependency.IsLoaded || !dependency.CanBeLoaded) continue;
            dependency.Load(loader);
        }
    }

    public AssemblyDefinition? FindLoadedAssembly(AssemblyName assemblyName)
    {
        foreach (var dependency in Files)
        {
            var assembly = dependency.LoadedAssembly;
            if (assembly != null && dependency.IsAvailable && assembly.Name.Name == assemblyName.Name)
            {
                return assembly;
            }
        }

        return null;
    }

    public TypeDefinition? FindLoadedType(string typeFullName, in List<string> excludedFiles)
    {
        foreach (var dependency in Files)
        {
            if (excludedFiles.Contains(dependency.Path)) continue;
            var type = dependency.LoadedAssembly?.Resolve(typeFullName);
            if (type == null) continue;
            return type;
        }

        return null;
    }

    private void AddFiles(List<string> existingPaths, params string[] files)
    {
        foreach (var filePath in files)
        {
            if (existingPaths.Contains(filePath))
            {
                throw new ArgumentException($"File {filePath} already added as dependency");
            }

            var file = new DependencyFile(filePath);
            Files.Add(file);
            existingPaths.Add(file.Path);
        }
    }

    private List<string> GetExistingPaths() => Files.Select(x => x.Path).ToList();
}