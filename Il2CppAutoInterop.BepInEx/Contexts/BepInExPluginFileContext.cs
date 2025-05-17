using Il2CppAutoInterop.Core.Interfaces;

namespace Il2CppAutoInterop.BepInEx.Contexts;

public class BepInExPluginFileContext : PostProcessingContext, IFileProcessorContext
{
    public string InputPath { get; }

    public BepInExPluginFileContext(PostProcessingContext options, string inputPath) : base(options)
    {
        InputPath = inputPath;
    }

    protected BepInExPluginFileContext(BepInExPluginFileContext context) : this(context, context.InputPath)
    {
    }

}