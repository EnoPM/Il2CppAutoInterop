using Il2CppAutoInterop.BepInEx.Contexts.Summaries;
using Il2CppAutoInterop.BepInEx.Interfaces;
using Il2CppAutoInterop.BepInEx.Utils;
using Il2CppAutoInterop.Cecil.Interfaces;
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