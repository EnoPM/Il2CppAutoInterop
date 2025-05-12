using System.Reflection;
using Il2CppAutoInterop.BepInEx;

namespace Il2CppAutoInterop.Compiler;

internal static class Program
{
    private static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            throw InvalidArguments(args);
        }

        var inputPath = args[0];
        
        Console.WriteLine($"Processing assembly: {inputPath}");
        
        var context = new BepInExContext(inputPath);

        var processor = new PluginAssemblyProcessor(context);
        processor.ProcessAsync().Wait();
    }

    private static Exception InvalidArguments(string[] _)
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var executableName = Path.GetFileName(assemblyLocation);

        var commandUsage = string.Join(' ',
            executableName,
            Required(Quoted(@"C:\input.dll"))
        );

        return new ArgumentException($"Correct usage: {commandUsage}");
    }

    private static string Quoted(string value) => $"\"{value}\"";
    private static string Optional(string value) => $"({value})";
    private static string Required(string value) => $"[{value}]";
}