using Il2CppAutoInterop.Cecil.Interfaces;
using Il2CppAutoInterop.Common;
using Il2CppAutoInterop.Common.Logging;
using Il2CppAutoInterop.Core.Interfaces;
using Mono.Cecil;

namespace Il2CppAutoInterop.Core.Processors;

public abstract class BaseFileProcessor<TContext>(TContext context) : BaseProcessor<TContext>(context)
    where TContext : class, IFileProcessorContext
{
    public IAssemblyLoader Loader => Context.Loader ??= CreateLoader();
    public AssemblyDefinition? LoadedAssembly { get; private set; }

    public override void Process()
    {
        if (LoadedAssembly == null) throw NotLoaded(nameof(Process));
        Process(LoadedAssembly);
    }

    public virtual void Load()
    {
        Context.Loader = Loader;
        try
        {
            var assembly = LoadAssembly();
            LoadedAssembly = assembly;
        }
        catch (Exception ex)
        {
            NotLoadedError(nameof(Load), ex);
            throw;
        }
    }
    
    protected abstract void Process(AssemblyDefinition assembly);

    protected virtual AssemblyDefinition LoadAssembly() => Loader.Load(Context.InputPath);

    protected virtual IAssemblyLoader CreateLoader() => new AssemblyLoader();

    public virtual void Save(string directoryPath)
    {
        if (LoadedAssembly == null) throw NotLoaded(nameof(Save));
        
        var fileName = Path.GetFileName(Context.InputPath);
        Logger.Instance.Log($"Saving assembly to {Path.Combine(directoryPath, fileName)}", ConsoleColor.Blue);
        LoadedAssembly.Write(Path.Combine(directoryPath, fileName));
    }
    
    private Exception NotLoaded(string source)
    {
        return ExceptionFactory.AssemblyNotYetLoaded<BaseProcessor<TContext>>(source, Context.InputPath);
    }
    
    private void NotLoadedError(string source, Exception ex)
    {
        ExceptionFactory.AssemblyNotYetLoadedError<BaseProcessor<TContext>>(source, Context.InputPath, ex);
    }
}